#include <xtl.h>
#include <wchar.h>
#include <stdio.h>
#include <time.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <xkelib.h>

#include "FtpDll.h"


HRESULT GetBytesString(BYTE* Data, UINT DataLen, CHAR* OutBuffer, UINT* OutLen) {

	// Check our lenghts
	if(*OutLen < (DataLen * 2))
		return S_FALSE;

	*OutLen = DataLen * 2;

	// Output into our buffer as hex
	CHAR hexChars[] = "0123456789ABCDEF";
	for(UINT x = 0, y = 0; x < DataLen; x++, y+=2) {
		OutBuffer[y] = hexChars[(Data[x] >> 4)];
		OutBuffer[y + 1] = hexChars[(Data[x] & 0x0F)];
	}

	// All done =)
	return S_OK;
}

// TODO: manage file sizes larger than 4 GB (usbs etc)
HRESULT InlineMd5( char *value ) {
	CHAR bytes[0x10]; CHAR byteStr[0x22];
	UINT outLen = 0x22;
	memset( bytes, 0, 0x10 ); 
	memset( byteStr, 0, 0x22 );
	XeCryptMd5( (const PBYTE)value, strlen(value), NULL, NULL, NULL, NULL, (PBYTE)bytes, 0x10 );
	memset(value, 0, 0x23);
	GetBytesString( (BYTE*)bytes, 0x10, byteStr, &outLen );
	strcpy_s(value, 0x22, byteStr);
	return S_OK;
}

VOID ParseDir(const char * szIn, char * szOut)
{
	int i = 0, j = 0;
	BOOL bDrive = TRUE;

	if(szIn[0] == '/')
		i = 1;

	for(;szIn[i];i++,j++)
	{
		if(szIn[i] == '/')
		{
			if(bDrive)
			{
				szOut[j++] = ':';
				szOut[j] = '\\';
				bDrive = FALSE;
			}
			else
				szOut[j] = '\\';
		}
		else
			szOut[j] = szIn[i];
	}

	szOut[j] = 0;
}

VOID Resolve(const char * szIn, const char * szDir, char * szOut)
{
	int i;

	// we can be parsing, /Something/ which is absolute, Something/ which is relative
	if(szIn[0] == '/') // Absolute
		strcpy(szOut, szIn);
	else
	{
		// Relative
		strcpy(szOut, szDir);
		strcat(szOut, szIn);
	}

	i = strlen(szOut) - 1;
	if(szOut[i] != '/')
	{
		szOut[i + 1] = '/';
		szOut[i + 2] = 0;
	}
}

CMD_PROC(CmdCdup)
{
	int i, j = -1;

	for(i = 0;psle->szDir[i];i++)
	{
		if(psle->szDir[i] == '/' && psle->szDir[i + 1])
			j = i;
	}

	if(j != -1)
		psle->szDir[j + 1] = 0;

	SendMsg(psle->sock, 250, "Current directory is %s", psle->szDir);

	return TRUE;
}

CMD_PROC(CmdCwd)
{
	char sz[0x200];
	int attr;

	if(szCmd[0] == '/' && !szCmd[1])
	{
		psle->szDir[0] = '/';
		psle->szDir[1] = 0;

		SendMsg(psle->sock, 250, "Current directory is /");
		return TRUE;
	}
	
	Resolve(szCmd, psle->szDir, sz);
	strcpy(szCmd, sz); // Put the full path into szCmd

	ParseDir(szCmd, sz); // Put the physical path into sz

	if((attr = GetFileAttributes(sz)) != 0xFFFFFFFF)
	{
		if(attr & FILE_ATTRIBUTE_DIRECTORY)
		{
			strcpy(psle->szDir, szCmd);
			SendMsg(psle->sock, 250, "Current directory is %s", szCmd);
		}
		else
			SendMsg(psle->sock, 550, "Not a directory");
	}
	else
		SendMsg(psle->sock, 550, "No such directory");

	return TRUE;
}

CMD_PROC(CmdDele)
{
	char sz[0x200];
	int i;

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);
	i = strlen(szCmd);
	if(szCmd[i - 1] == '\\')
		szCmd[i - 1] = 0;

	if(DeleteFile(szCmd))
		SendMsg(psle->sock, 250, "File deleted");
	else
		SendMsg(psle->sock, 550, "Could not delete");

	return TRUE;
}

CMD_PROC(CmdFeat)
{
	SendMsg(psle->sock, 211, "Extensions supported:");
	SendLine(psle->sock, " CDUP\r\n");
	SendLine(psle->sock, " UTF8\r\n");
	SendLine(psle->sock, " PLAY\r\n");
	SendMsg(psle->sock, 211, " END");
	return TRUE;
}

char *szMonths[] = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

CMD_PROC(ContNlst)
{
	// Time strings
	int ret = 0;
	struct stat statFileStats;
	char filepath[512] = "";
	char fullpath[512] = "";
	char timeStr[ 100 ] = "";
	struct tm locTime;

	unsigned i;
	WIN32_FILE_ATTRIBUTE_DATA fad;
	char sz[0x200];
	char e, r, w, d;
	SYSTEMTIME now; // sys, now;
	HANDLE hFind;
	WIN32_FIND_DATA wfd;

	// TODO: Remote directory param

	if(!strcmpi(psle->szDir, "/"))
	{
		// We list mount points here
		for(i = 0;i < dwDriveCount;i++)
		{
			sprintf_s(sz, 0x200, "%s:\\", szDrives[i]);
			if(GetFileAttributesEx(sz, GetFileExInfoStandard, &fad))
			{
				e = 'x';
				r = 'r';
				w = 'w';
				d = '-';

				if(fad.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
					d = 'd';
				if(fad.dwFileAttributes & FILE_ATTRIBUTE_ARCHIVE)
					e = '-';
				if(fad.dwFileAttributes & FILE_ATTRIBUTE_READONLY)
					w = '-';
				if(fad.dwFileAttributes & FILE_ATTRIBUTE_DEVICE)
					r = '-';
				
				//GetSystemTime(&now);
				//FileTimeToSystemTime(&fad.ftLastAccessTime, &sys);

				SendLine(psle->sockData, "%c%c%c%c%c%c%c%c%c%c   1 root root %13d Jan 01  2000 %s\r\n",d,r,w,e,r,w,e,r,w,e,0,szDrives[i]);
			}
		}
	}
	else
	{
		// List entries
		ParseDir(psle->szDir, sz);
		
		memset( filepath, 0, 512 );

		strcat( filepath, sz );
		strcat(sz, "*");

		hFind = FindFirstFile(sz, &wfd);
		if(hFind != INVALID_HANDLE_VALUE)
		{
			do
			{
				e = 'x';
				r = 'r';
				w = 'w';
				d = '-';

				if(wfd.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
					d = 'd';
				if(wfd.dwFileAttributes & FILE_ATTRIBUTE_ARCHIVE)
					e = '-';
				if(wfd.dwFileAttributes & FILE_ATTRIBUTE_READONLY)
					w = '-';
				if(wfd.dwFileAttributes & FILE_ATTRIBUTE_DEVICE)
					r = '-'; 


				// Get stat on file
				memset( fullpath, 0, 512 );
				strcat( fullpath, filepath );
				strcat( fullpath, wfd.cFileName );
				 
				stat( fullpath, &statFileStats );

				ret = localtime_s( &locTime, &statFileStats.st_mtime );
				if( ret > 0 ) ret = localtime_s( &locTime, &statFileStats.st_atime );
				if( ret > 0 ) ret = localtime_s( &locTime, &statFileStats.st_ctime );

/*				if( FileTimeToLocalFileTime( &fad.ftLastAccessTime, &localFileTime ) == FALSE )
				{
					DWORD x = GetLastError();
					DbgPrint( "FileTimeToLocalFileTime Error Code:  %d\n", x );
				}
				GetSystemTime(&now);
				if( FileTimeToSystemTime(&localFileTime, &sys) == FALSE ) {
					DWORD x = GetLastError();
					DbgPrint( "FileTimeToSystemTime Error Code:  %d\n, FileTime:  %08X %08X", x, localFileTime.dwHighDateTime, localFileTime.dwLowDateTime );
				}
				*/
				GetSystemTime(&now);

				
				if((now.wYear * 12 + now.wMonth) - ((locTime.tm_year + 1900) * 12 + locTime.tm_mon) < 6)
				{
					strftime(timeStr, 100, "%b %d %H:%M",  &locTime);
				}
				else 
				{
					strftime(timeStr, 100, "%b %d %Y",  &locTime);
				}
				SendLine(psle->sockData, "%c%c%c%c%c%c%c%c%c%c   1 root root %13d %s %s\r\n",d,r,w,e,r,w,e,r,w,e,wfd.nFileSizeLow, timeStr, wfd.cFileName);
				
			} while(FindNextFile(hFind, &wfd));

			FindClose(hFind);
		}
	}

	SendMsg(psle->sock, 226, "Transfer complete");
	return TRUE;
}

CMD_PROC(CmdNlst)
{
	psle->csState = WaitingForConnection;
	psle->ConnectionProc = ContNlst;

	SendMsg(psle->sock, 150, "Opening connection");
	return TRUE;
}

CMD_PROC(CmdMkd)
{
	char sz[0x200];

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);

	if(CreateDirectory(szCmd, NULL))
		SendMsg(psle->sock, 257, "Directory created");
	else
		SendMsg(psle->sock, 550, "Cannot create directory");

	return TRUE;
}

CMD_PROC(CmdMkts)
{
	SendMsg(psle->sock, 200, "Toast has been made");
	return TRUE;
}

CMD_PROC(CmdNoop)
{
	SendMsg(psle->sock, 200, "ori r0, r0, 0");
	return TRUE;
}

CMD_PROC(CmdLaunch)
{
	char sz[0x200];
	int i;

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);
	i = strlen(szCmd);
	if(szCmd[i - 1] == '\\')
		szCmd[i - 1] = 0;

	SendMsg(psle->sock, 200, "Launching Game");

	if( pLaunchGame ) 
		pLaunchGame( szCmd );

	return TRUE;
}

CMD_PROC(CmdPass)
{
	if(psle->bLoggedIn)
	{
		SendMsg(psle->sock, 530, "Already logged in");
		return TRUE;
	}

	if(!psle->szUser[0])
	{
		SendMsg(psle->sock, 530, "Need username");
		return TRUE;
	}

	EnterCriticalSection(&csLoginInfo);
	InlineMd5(szCmd);
	if(!stricmp(szCmd, szPass))
	{
		psle->bLoggedIn = TRUE;
		SendMsg(psle->sock, 230, "User logged in");
		return TRUE;
	}
	else
	{
		psle->szUser[0] = 0;
		SendMsg(psle->sock, 530, "Login failed: input = %s, pass = %s", szCmd, szPass);
		return TRUE;
	}

	LeaveCriticalSection(&csLoginInfo);
}

CMD_PROC(CmdPasv)
{
	XNADDR xna;
	BOOL bReuse = TRUE;
	SOCKADDR_IN sin;

	XNetGetTitleXnAddr(&xna);

	// Passive mode is where we listen for a connection from them, so lets just setup the socket
	if(psle->sockPasv != INVALID_SOCKET)
	{
		// We are already in passive mode, just signal back
		SendMsg(psle->sock, 227, "Entering Passive Mode (%d,%d,%d,%d,%d,%d)",
			xna.ina.S_un.S_un_b.s_b1,
			xna.ina.S_un.S_un_b.s_b2,
			xna.ina.S_un.S_un_b.s_b3,
			xna.ina.S_un.S_un_b.s_b4,
			psle->dataPort >> 8,
			psle->dataPort & 0xFF);
		return TRUE;
	}

	// TODO: Error checking and stuff
	psle->sockPasv = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
	setsockopt(psle->sockPasv, SOL_SOCKET, 0x5801, (char*)&bReuse, sizeof(bReuse));
	setsockopt(psle->sockPasv, SOL_SOCKET, SO_REUSEADDR, (char*)&bReuse, sizeof(bReuse));

	sin.sin_family = AF_INET;
	sin.sin_addr.s_addr = INADDR_ANY;
	sin.sin_port = psle->dataPort;

	if(bind(psle->sockPasv, (SOCKADDR*)&sin, sizeof(sin)))
	{
		closesocket(psle->sockPasv);
		psle->sockPasv = INVALID_SOCKET;

		SendMsg(psle->sock, 550, "Unable to enter Passive Mode");
		return TRUE;
	}

	if(listen(psle->sockPasv, SOMAXCONN))
	{
		closesocket(psle->sockPasv);
		psle->sockPasv = INVALID_SOCKET;

		SendMsg(psle->sock, 550, "Unable to enter Passive Mode");
		return TRUE;
	}

	SendMsg(psle->sock, 227, "Entering Passive Mode (%d,%d,%d,%d,%d,%d)",
		xna.ina.S_un.S_un_b.s_b1,
		xna.ina.S_un.S_un_b.s_b2,
		xna.ina.S_un.S_un_b.s_b3,
		xna.ina.S_un.S_un_b.s_b4,
		psle->dataPort >> 8,
		psle->dataPort & 0xFF);

	return TRUE;
}

CMD_PROC(CmdPwd)
{
	SendMsg(psle->sock, 257, "\"%s\"", psle->szDir);
	return TRUE;
}

CMD_PROC(CmdQuit)
{
	SendMsg(psle->sock, 221, "Goodbye");
	return FALSE;
}

CMD_PROC(ContRetr)
{
	int read = 0;

	// This is a pretty simple setup, we just read into the buffer when its empty, and in the meantime we just stream the data over
	// the buffer position is used as an indicator of how many bytes are left, just shift after sending
	if(psle->dwDataLen == 0)
	{
		psle->dwDataPos = 0;
		ReadFile(psle->hFile, psle->szData, sizeof(psle->szData), &psle->dwDataLen, NULL);
	}
	
	if(psle->dwDataLen == 0)
	{
		// We are done transferring
		SendMsg(psle->sock, 226, "File transferred");
		return TRUE;
	}

	if(psle->bCanWrite)
	{
		read = send(psle->sockData, psle->szData + psle->dwDataPos, psle->dwDataLen, 0);
		if(read <= 0)
		{
			psle->dwDataPos = 0;
			psle->dwDataLen = 0;
			SendMsg(psle->sock, 550, "Connection interrupted");
			return TRUE;
		}

		psle->dwDataLen -= read;
		psle->dwDataPos += read;
	}

	return FALSE;
}

CMD_PROC(CmdRetr)
{
	char sz[0x200];
	int i;

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);
	i = strlen(szCmd);
	if(szCmd[i - 1] == '\\')
		szCmd[i - 1] = 0;

	psle->hFile = CreateFile(szCmd, GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, 0, NULL);

	if(psle->hFile != INVALID_HANDLE_VALUE)
	{
		psle->csState = WaitingForConnection;
		psle->ConnectionProc = ContRetr;

		SendMsg(psle->sock, 150, "Waiting for connection");
	}
	else
		SendMsg(psle->sock, 550, "Could not open file");

	return TRUE;
}

CMD_PROC(CmdRmd)
{
	char sz[0x200];

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);

	if(RemoveDirectory(szCmd))
		SendMsg(psle->sock, 250, "Directory removed");
	else
		SendMsg(psle->sock, 550, "Cannot remove directory");

	return TRUE;
}

CMD_PROC(CmdRnfr)
{
	char sz[0x200];

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);

	psle->bRename = TRUE;
	strcpy(psle->szRename, szCmd);

	SendMsg(psle->sock, 350, "RNFR accepted");

	return TRUE;
}

CMD_PROC(CmdRnto)
{
	char sz[0x200];
	int i;

	if(!psle->bRename)
	{
		SendMsg(psle->sock, 503, "Cannot rename file");
		return TRUE;
	}

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);

	i = strlen(szCmd) - 1;
	if(szCmd[i] == '\\')
		szCmd[i] = 0;
	i = strlen(psle->szRename) - 1;
	if(psle->szRename[i] == '\\')
		psle->szRename[i] = 0;

	if(MoveFile(psle->szRename, szCmd))
		SendMsg(psle->sock, 250, "File renamed");
	else
		SendMsg(psle->sock, 550, "Failed to rename file");

	psle->bRename = FALSE;

	return TRUE;
}

CMD_PROC(CmdSize)
{
	WIN32_FILE_ATTRIBUTE_DATA fad;
	char sz[0x200];

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);

	if(GetFileAttributesEx(szCmd, GetFileExInfoStandard, &fad))
	{
		if(fad.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			SendMsg(psle->sock, 550, "Not a file");
		else
			SendMsg(psle->sock, 213, "%d", fad.nFileSizeLow);
	}
	else
		SendMsg(psle->sock, 550, "File not found");

	return TRUE;
}

CMD_PROC(CmdStat)
{
	SendMsg(psle->sock, 211, "Aurora FtpDll");
	return TRUE;
}

CMD_PROC(ContStor)
{
	int i;
	DWORD j, k;
	DWORD written;

	if(psle->bDataRead)
	{
		i = recv(psle->sockData, psle->szData + psle->dwDataPos, sizeof(psle->szData) - psle->dwDataPos, 0);

		if(i <= 0)
		{
			if(psle->dwDataPos)
			{
				if(psle->bFlash)
				{
					for(j = 0;j < psle->dwDataPos;j += 0x4000)
					{
						k = psle->dwDataPos - j;
						if(k > 0x4000)
							k = 0x4000;

						WriteFile(psle->hFile, psle->szData + j, k, &written, NULL);
					}
				}
				else
					WriteFile(psle->hFile, psle->szData, psle->dwDataPos, &written, NULL);
				psle->dwDataPos = 0;
			}

			SendMsg(psle->sock, 226, "File transferred");
			return TRUE; // Signal completion
		}

		psle->dwDataPos += i;

		if(psle->dwDataPos == sizeof(psle->szData))
		{
			if(psle->bFlash)
			{
				for(j = 0;j < psle->dwDataPos;j += 0x4000)
				{
					k = psle->dwDataPos - j;
					if(k > 0x4000)
						k = 0x4000;

					WriteFile(psle->hFile, psle->szData + j, k, &written, NULL);
				}
			}
			else
				WriteFile(psle->hFile, psle->szData, psle->dwDataPos, &written, NULL);
			psle->dwDataPos = 0;
		}
	}

	return FALSE;
}

CMD_PROC(CmdStor)
{
	char sz[0x200];
	int i;

	Resolve(szCmd, psle->szDir, sz);
	ParseDir(sz, szCmd);

	// Remove the trailing slash, this is a file we are making here
	i = strlen(szCmd);
	if(szCmd[i - 1] == '\\')
		szCmd[i - 1] = 0;

	psle->hFile = CreateFile(szCmd, GENERIC_WRITE, 0, NULL, CREATE_ALWAYS, 0, 0);

	if(psle->hFile != INVALID_HANDLE_VALUE)
	{
		if(!strnicmp(szCmd, "Flash:", 6))
			psle->bFlash = TRUE;
		else
			psle->bFlash = FALSE;

		psle->csState = WaitingForConnection;
		psle->ConnectionProc = ContStor;

		SendMsg(psle->sock, 150, "Opening connection");
	}
	else
		SendMsg(psle->sock, 550, "Could not create file");

	return TRUE;
}

CMD_PROC(CmdSyst)
{
	// RFC 959
	SendMsg(psle->sock, 215, "UNIX Type: L8");
	return TRUE;
}

CMD_PROC(CmdType)
{
	SendMsg(psle->sock, 200, "Type set to %s", szCmd);
	return TRUE;
}

CMD_PROC(CmdUser)
{
	if(psle->bLoggedIn)
	{
		SendMsg(psle->sock, 530, "Already logged in");
		return TRUE;
	}

	EnterCriticalSection(&csLoginInfo);

	if(!stricmp(szCmd, szUser))
	{
		strcpy(psle->szUser, szCmd);
		SendMsg(psle->sock, 331, "User %s OK, need password", psle->szUser);
	}
	else
	{
		psle->szUser[0] = 0;
		SendMsg(psle->sock, 332, "User not OK");
	}

	LeaveCriticalSection(&csLoginInfo);
	return TRUE;
}

COMMAND CommandList[] =
{
	{ "CDUP",	CmdCdup,	0 },
	{ "CWD",	CmdCwd,		0 },
	{ "DELE",	CmdDele,	0 },
	{ "FEAT",	CmdFeat,	0 },
	{ "LIST",	CmdNlst,	0 },
	{ "MKD",	CmdMkd,		0 },
	{ "MKTS",	CmdMkts,	0 },
	{ "NLST",	CmdNlst,	0 },
	{ "NOOP",	CmdNoop,	0 },
	{ "PASS",	CmdPass,	1 },
	{ "PASV",	CmdPasv,	0 },
	{ "PWD",	CmdPwd,		0 },
	{ "QUIT",	CmdQuit,	1 },
	{ "RETR",	CmdRetr,	0 },
	{ "RMD",	CmdRmd,		0 },
	{ "RNFR",	CmdRnfr,	0 },
	{ "RNTO",	CmdRnto,	0 },
	{ "SIZE",	CmdSize,	0 },
	{ "STAT",	CmdStat,	0 },
	{ "STOR",	CmdStor,	0 },
	{ "SYST",	CmdSyst,	0 },
	{ "TYPE",	CmdType,	0 },
	{ "USER",	CmdUser,	1 },
	{ "XMKD",	CmdMkd,		0 },
	{ "PLAY",   CmdLaunch,  0 },
};
DWORD CommandCount = sizeof(CommandList) / sizeof(COMMAND);