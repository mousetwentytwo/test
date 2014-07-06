// FtpDll.cpp : Defines the entry point for the application.
//

#include <xtl.h>
#include <xboxmath.h>
#include <stdio.h>
#include <time.h>

#include "FtpDll.h"

// The dll's thread
HANDLE hThread;
DWORD dwThreadId;

// The processor to sit on
DWORD dwProcessor;

// If we are running
BOOL bIsRunning;

// If we should stop now
BOOL bShouldStop;

// Our socket list
LIST_ENTRY leSockList;

// The login critical section
CRITICAL_SECTION csLoginInfo;

// The current username and password
char szUser[48];
char szPass[48];

// Port to run on
int dPort;

// Static command buffer
char szCmd[0x200];

// Connection count
DWORD dwConnCount;

// The used data ports
BOOL dataPorts[MAX_CONN_COUNT];

// The active drives
char szDrives[32][32];
DWORD dwDriveCount;

LAUNCHGAME_FUNC pLaunchGame = NULL; 

BOOL SendLinePlain(SOCKET sock, const char * fmt)
{
	if(send(sock, fmt, strlen(fmt), 0) == SOCKET_ERROR)
	{
		return FALSE;
	}

	return TRUE;
}

BOOL SendLine(SOCKET sock, const char * fmt, ...)
{
	char buf[0x200];
	va_list list;

	va_start(list, fmt);
	vsprintf_s(buf, 0x200, fmt, list);
	va_end(list);

	return SendLinePlain(sock, buf);
}

BOOL SendMsgPlain(SOCKET sock, int code, const char * message)
{
	char buf[0x200];

	sprintf_s(buf, 0x200, "%i %s\r\n", code, message);

	return SendLinePlain(sock, buf);
}

BOOL SendMsg(SOCKET sock, int code, const char * fmt, ...)
{
	char buf[0x200];
	va_list list;

	va_start(list, fmt);
	vsprintf_s(buf, 0x200, fmt, list);
	va_end(list);

	return SendMsgPlain(sock, code, buf);
}

VOID RemoveSocket(PSOCKET_LIST_ENTRY psle)
{
	RemoveEntryList(&psle->leList);

	if(psle->sock != INVALID_SOCKET)
	{
		shutdown(psle->sock, SD_BOTH);
		closesocket(psle->sock);
		psle->sock = INVALID_SOCKET;
	}
	if(psle->sockData != INVALID_SOCKET)
	{
		shutdown(psle->sockData, SD_BOTH);
		closesocket(psle->sockData);
		psle->sockData = INVALID_SOCKET;
	}
	if(psle->sockPasv != INVALID_SOCKET)
	{
		closesocket(psle->sockPasv);
		psle->sockPasv = INVALID_SOCKET;
	}
	if(psle->hFile != INVALID_HANDLE_VALUE)
	{
		CloseHandle(psle->hFile);
		psle->hFile = INVALID_HANDLE_VALUE;
	}

	dataPorts[psle->dataPort - DATA_PORT_START] = FALSE;

	dwConnCount--;

	free(psle);
}

BOOL DoReadWrite(PSOCKET_LIST_ENTRY psle)
{
	unsigned i = 0, j = 0, cch = 0;
	BOOL cmd = FALSE;
	int addrlen;
	BOOL canTimeout = TRUE;

	if(psle->csState == Error)
		return FALSE;

	// Check for processing
	if(psle->csState == ProcessingData)
	{
		if(psle->ConnectionProc(psle))
		{
			psle->csState = WaitingForCommand;

			// Clean out the socket
			if(psle->sockData != INVALID_SOCKET)
			{
				shutdown(psle->sockData, SD_BOTH);
				closesocket(psle->sockData);
				psle->sockData = INVALID_SOCKET;
			}

			if(psle->hFile != INVALID_HANDLE_VALUE)
			{
				CloseHandle(psle->hFile);
				psle->hFile = INVALID_HANDLE_VALUE;
			}
		}
	}

	// Check up on the passive listener
	if(psle->csState == WaitingForConnection)
	{
		if(psle->bCanAccept)
		{
			// We got a connection! lol
			addrlen = sizeof(psle->dataAddr);
			psle->sockData = accept(psle->sockPasv, (SOCKADDR*)&psle->dataAddr, &addrlen);

			// Run the proc that they requested
			if(psle->ConnectionProc(psle))
			{
				psle->csState = WaitingForCommand;

				// Clean out the socket
				if(psle->sockData != INVALID_SOCKET)
				{
					shutdown(psle->sockData, SD_BOTH);
					closesocket(psle->sockData);
					psle->sockData = INVALID_SOCKET;
				}

				if(psle->hFile != INVALID_HANDLE_VALUE)
				{
					CloseHandle(psle->hFile);
					psle->hFile = INVALID_HANDLE_VALUE;
				}
			}
			else
				psle->csState = ProcessingData;		

			canTimeout = FALSE;
		}
		else if(GetTickCount() - psle->dwPasvTimeout >= PASSIVE_TIMEOUT)
		{
			// We timed out
			if(psle->sockData != INVALID_SOCKET)
			{
				shutdown(psle->sockData, SD_BOTH);
				closesocket(psle->sockData);
				psle->sockData = INVALID_SOCKET;
			}

			psle->csState = WaitingForCommand;

			SendMsg(psle->sock, 425, "Cannot open data connection");
		}
	}

	if(psle->csState == Connecting && psle->bCanWrite)
	{
		if(SendMsg(psle->sock, 220, "FtpDll Ready"))
			psle->csState = WaitingForCommand;
		else
			psle->csState = Error;

		canTimeout = FALSE;
	}
	else if(psle->csState == WaitingForCommand)
	{
		// TODO: something about the overflow that might be caused here

		if(psle->bCanRead)
		{
			i = recv(psle->sock, psle->szBuf + psle->dwBufPos, sizeof(psle->szBuf) - psle->dwBufPos, 0);
		}
		else
			j = 1;

		 if(j == 0 && (i <= 0 || psle->dwBufPos + i > 0x200))
			 psle->csState = Error;
		 else
		 {
			 szCmd[0] = 0;

			 cch = psle->dwBufPos + i;
			 for(j = 0;j < psle->dwBufPos + i;j++)
			 {
				 if(psle->szBuf[j] == '\n')
				 {
					 cmd = TRUE;

					 memcpy(szCmd, psle->szBuf, j);
					 szCmd[j] = 0;

					 cch -= j + 1;
					 memmove(psle->szBuf, psle->szBuf + j + 1, sizeof(psle->szBuf) - j - 1);
					 break;
				 }
				 else if(psle->szBuf[j] == '\r')
				 {
					 cmd = TRUE;

					 memcpy(szCmd, psle->szBuf, j);
					 szCmd[j] = 0;

					 if(psle->szBuf[j + 1] == '\n')
					 {
						 cch -= j + 2;
						 memmove(psle->szBuf, psle->szBuf + j + 2, sizeof(psle->szBuf) - j - 2);
					 }
					 else
					 {
						 cch -= j + 1;
						 memmove(psle->szBuf, psle->szBuf + j + 1, sizeof(psle->szBuf) - j - 1);
					 }
					 break;
				 }
			 }

			 psle->dwBufPos = cch;

			 if(cmd)
			 {
				 if(szCmd[0] == 0)
				 {
					 SendMsg(psle->sock, 500, "?");
				 }
				 else
				 {
					for(j = 0;szCmd[j] != ' ' && szCmd[j] != 0;j++);

					for(i = 0;i < CommandCount;i++)
					{
						if(!strnicmp(szCmd, CommandList[i].szName, j))
						{
							// Copy the command arguments down in the buffer so the command proc doesn't have to worry about it
							if(j +1 < sizeof(szCmd))
								memmove(szCmd, szCmd + j + 1, sizeof(szCmd) - j - 1);

							if(!CommandList[i].bLoginNotRequired && !psle->bLoggedIn)
							{
								SendMsg(psle->sock, 530, "Not logged in");
							}
							else if(!CommandList[i].Proc(psle))
								psle->csState = Error;

							if(psle->csState == WaitingForConnection)
								psle->dwPasvTimeout = GetTickCount();

							break;
						}
					 }

					if(i == CommandCount)
					{
						if(psle->bLoggedIn)
							SendMsg(psle->sock, 502, "Command not found");
						else
							SendMsg(psle->sock, 530, "Not logged in");
					}

					canTimeout = FALSE;
				 }
			 }
		 }
	}

	if(canTimeout)
	{
		if(GetTickCount() - psle->dwTimeout > CONNECTION_TIMEOUT)
		{
			// We just timed out
			psle->csState = Error;

			SendMsg(psle->sock, 421, "Timed out");
		}
	}
	else
	{
		psle->dwTimeout = GetTickCount();
	}

	return TRUE;
}

//
// Usage: SetThreadName (-1, "MainThread");
//
typedef struct tagTHREADNAME_INFO
{
	DWORD dwType; // must be 0x1000
	LPCSTR szName; // pointer to name (in user addr space)
	DWORD dwThreadID; // thread ID (-1=caller thread)
	DWORD dwFlags; // reserved for future use, must be zero
} THREADNAME_INFO;

void SetThreadName( DWORD dwThreadID, LPCSTR szThreadName)
{
	THREADNAME_INFO info;
	info.dwType = 0x1000;
	info.szName = szThreadName;
	info.dwThreadID = dwThreadID;
	info.dwFlags = 0;

	__try
	{
		RaiseException( 0x406D1388, 0, sizeof(info)/sizeof(DWORD), (DWORD*)&info );
	}
	__except(EXCEPTION_CONTINUE_EXECUTION)
	{
	}
}

// The thread proc
ULONG __stdcall FtpThread(LPVOID lpIgnored)
{
	// Setup some vars
	SOCKET sockServ = INVALID_SOCKET;
	FD_SET readfds, writefds, errorfds;
	PLIST_ENTRY ple;
	PSOCKET_LIST_ENTRY psle;
	SOCKADDR_IN saddr;
	int saddrlen;
	SOCKET sock;
	BOOL bReuse;
	XNADDR xaddr;
	LINGER linger;
	int i;
	struct timeval timeout;

	SetThreadName(-1, "Ftp Server");

	linger.l_linger = 0;
	linger.l_onoff = 1;

	timeout.tv_sec = 0;
	timeout.tv_usec = 0;

	for(;;)
	{
		// Check to see if we should stop
		if(bShouldStop)
		{

			// Shut down everything
			if(sockServ != INVALID_SOCKET)
			{
				shutdown(sockServ, SD_BOTH);
				closesocket(sockServ);

				sockServ = INVALID_SOCKET;
			}

			ple = leSockList.Flink;
			while(ple != &leSockList)
			{
				psle = CONTAINING_RECORD(ple, SOCKET_LIST_ENTRY, leList);
				ple = ple->Flink;

				RemoveSocket(psle);
			}

			DbgPrint("Server stopped");
			bIsRunning = FALSE;
			bShouldStop = FALSE;

			ExitThread(0);
		}

		if(sockServ == INVALID_SOCKET)
		{
			// We need to initialize everything here
			sockServ = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);

			if(sockServ == INVALID_SOCKET)
			{
				DbgPrint("Server is invalid!");
				DebugBreak();
			}

			bReuse = TRUE;
			if(setsockopt(sockServ, SOL_SOCKET, 0x5801, (char*)&bReuse, sizeof(bReuse)))
			{
				DbgPrint("Setsockopt failed!");
				DebugBreak();
			}

			setsockopt(sockServ, SOL_SOCKET, SO_LINGER, (char*)&linger, sizeof(linger));
			setsockopt(sockServ, SOL_SOCKET, SO_REUSEADDR, (char*)&bReuse, sizeof(bReuse));

			saddr.sin_family = AF_INET;
			saddr.sin_port = dPort;
			saddr.sin_addr.s_addr = INADDR_ANY;
			if(bind(sockServ, (SOCKADDR*)&saddr, sizeof(saddr)))
			{
				DbgPrint("Bind failed!");
				DebugBreak();
			}

			if(listen(sockServ, SOMAXCONN))
			{
				DbgPrint("Listen failed!");
				DebugBreak();
			}

			DbgPrint("Server started");

			XNetGetTitleXnAddr(&xaddr);
			DbgPrint("Listening on %d.%d.%d.%d:21", 
				xaddr.ina.S_un.S_un_b.s_b1,
				xaddr.ina.S_un.S_un_b.s_b2,
				xaddr.ina.S_un.S_un_b.s_b3,
				xaddr.ina.S_un.S_un_b.s_b4);
		}

		// Check for io on the sockets
		readfds.fd_count = 0;
		writefds.fd_count = 0;
		errorfds.fd_count = 0;

		FD_SET(sockServ, &readfds);
		FD_SET(sockServ, &errorfds);

		ple = leSockList.Flink;
		while(ple != &leSockList)
		{
			psle = CONTAINING_RECORD(ple, SOCKET_LIST_ENTRY, leList);
			ple = ple->Flink;

			FD_SET(psle->sock, &readfds);
			FD_SET(psle->sock, &writefds);
			FD_SET(psle->sock, &errorfds);

			if(psle->sockPasv != INVALID_SOCKET)
				FD_SET(psle->sockPasv, &readfds);
			if(psle->sockData != INVALID_SOCKET)
			{
				FD_SET(psle->sockData, &readfds);
				FD_SET(psle->sockData, &writefds);
			}
		}

		// Let other threads do stuff too!
		Sleep(0);

		// Wait until we can do some socket io
		select(0, &readfds, &writefds, &errorfds, &timeout);

		// TODO: use the error fd set
		
		// Check for incoming connections
		if(FD_ISSET(sockServ, &readfds))
		{
			saddrlen = sizeof saddr;

			sock = accept(sockServ, (SOCKADDR*)&saddr, &saddrlen);

			if(sock != INVALID_SOCKET)
			{
				if(dwConnCount < MAX_CONN_COUNT)
				{
					setsockopt(sock, SOL_SOCKET, SO_LINGER, (char*)&linger, sizeof(linger));

					// Allocate and add to list
					psle = (PSOCKET_LIST_ENTRY)malloc(sizeof(SOCKET_LIST_ENTRY));
				
					if(!psle)
					{
						DbgPrint("Out of memory!");
						DebugBreak();
					}

					ZeroMemory(psle, sizeof(SOCKET_LIST_ENTRY));

					psle->szDir[0] = '/';

					psle->addr = saddr;
					psle->sock = sock;

					psle->sockData = INVALID_SOCKET;
					psle->sockPasv = INVALID_SOCKET;
					
					psle->hFile = INVALID_HANDLE_VALUE;

					for(i = 0;i < MAX_CONN_COUNT;i++)
						if(!dataPorts[i])
							break;

					psle->dwTimeout = GetTickCount();

					psle->dataPort = i + DATA_PORT_START;
					dataPorts[i] = TRUE;

					InsertHeadList(&leSockList, &psle->leList);

					dwConnCount++;
				}
				else
				{
					SendMsg(sock, 421, "Too many connections");
				}
			}
		}

		// Check the list sockets
		ple = leSockList.Flink;
		while(ple != &leSockList)
		{
			psle = CONTAINING_RECORD(ple, SOCKET_LIST_ENTRY, leList);
			ple = ple->Flink;

			// Mark as writable/readable
			if(FD_ISSET(psle->sock, &readfds))
				psle->bCanRead = TRUE;
			else
				psle->bCanRead = FALSE;

			if(FD_ISSET(psle->sock, &writefds))
				psle->bCanWrite = TRUE;
			else
				psle->bCanWrite = FALSE;

			if(FD_ISSET(psle->sockPasv, &readfds))
				psle->bCanAccept = TRUE;
			else
				psle->bCanAccept = FALSE;

			if(FD_ISSET(psle->sockData, &readfds))
				psle->bDataRead = TRUE;
			else
				psle->bDataRead = FALSE;

			if(FD_ISSET(psle->sockData, &writefds))
				psle->bDataWrite = TRUE;
			else
				psle->bDataWrite = FALSE;

			// Do socket stuff
			if(!DoReadWrite(psle))
				RemoveSocket(psle); // If it returns false, thats signaling that we should remove it from the list
		}
	}
}

// Outputs some debug info
#ifdef _DEBUG
VOID DbgPrint(const char * fmt, ...)
{
	char buf[0x200];
	va_list list;

	va_start(list, fmt);
	vsprintf_s(buf, 0x200, fmt, list);
	va_end(list);

	printf("[FtpDll] %s\n", buf);
}
#endif

// Stops the server, returns TRUE on success
BOOL StopServ()
{
	if(!bIsRunning)
		return FALSE;

	bShouldStop = TRUE;
	return TRUE;
}

// Starts the server, returns TRUE on success
BOOL StartServ()
{
	if (bIsRunning)
		return FALSE;
	
	bIsRunning = TRUE;
	bShouldStop = FALSE;
	ResumeThread(hThread);

	return TRUE;
}

BOOL APIENTRY DllMain(HANDLE hModule, DWORD dwReason, LPVOID lpReserved)
{
	if(dwReason == DLL_PROCESS_ATTACH)
	{
		DbgPrint("FtpDll loaded");

		// Do some init
		dwProcessor = 4;
		bIsRunning = FALSE;
		bShouldStop = FALSE;
		dwConnCount = 0;
		InitializeCriticalSection(&csLoginInfo);
		leSockList.Flink = leSockList.Blink = &leSockList;
		dwDriveCount = 0;

		ZeroMemory(szUser, sizeof(szUser));
		ZeroMemory(szPass, sizeof(szPass));

		ZeroMemory(dataPorts, sizeof(dataPorts));

		//#ifdef _DEBUG
		strcpy(szUser, "xbox");
		strcpy(szPass, "xbox");
		dPort = 21;
		//#endif

		// Create the thread
		hThread = CreateThread(NULL, 0, FtpThread, 0, CREATE_SUSPENDED, &dwThreadId);
		XSetThreadProcessor(hThread, dwProcessor);
	}

	return TRUE;
}