#include "stdafx.h"
#include "FTPServer.h"
#include "../Generic/xboxtools.h"
#include "../Settings/Settings.h"

CFTPServer::CFTPServer(void)
{
	m_CurrentStatus = "";
	m_DownBytesTranfered =0;
	m_UpBytesTranfered =0;

	TimerManager::getInstance().add(*this,1000);
	NetworkMonitor::getInstance().AddObserver(*this);
	if (NetworkMonitor::getInstance().HasInternetConnection()) {
		handleNetworkConnected();
	}
	
	Connected = FALSE;
	Exit = FALSE;
	
}

void CFTPServer::handleNetworkConnected() {
	xboxip = NetworkMonitor::getInstance().GetIpAddressA();

	LOG("FTPServer", "XNetGetTitleXnAddr returned %s",xboxip.c_str());
	
	port = 21; 
	//	ftpuser = SETTINGS::getInstance().getFtpUser(); 
	//	ftppass = SETTINGS::getInstance().getFtpPass();
	Connected = TRUE;
	CreateThread(CPU3_THREAD_1);
	Exit = FALSE;
}


void CFTPServer::handleNetworkDisconnected() {
	Connected = FALSE;
	Exit = TRUE;
}

void CFTPServer::TestSocket()
{
}
bool CFTPServer::HasActiveConnection()
{
	bool retVal = false;
	for(unsigned int x=0;x<Conns.size();x++)
	{
		CFTPServerConn* conn = Conns[x];
		if(conn->isActive())
		{
			retVal = true;
			break;
		}
	}
	return retVal;
}
unsigned long CFTPServer::Process(void* parameter)
{
	SetThreadName("FTP Server");
	SOCKET server;

	sockaddr_in local;
	local.sin_family=AF_INET;
	local.sin_addr.s_addr=INADDR_ANY;
	local.sin_port=htons((u_short)port);

    XferPortStart = XferPortRange = XferPort = 0;

	server=socket(AF_INET,SOCK_STREAM,IPPROTO_TCP);

	if(server==INVALID_SOCKET)
	{
		LOG("FTPServer",  "INVALID SOCKET!");
		return 0;
	}

	// after setting these undocumented flags on a socket they should then run unencrypted
	BOOL bBroadcast = TRUE;

	if( setsockopt(server, SOL_SOCKET, 0x5802, (PCSTR)&bBroadcast, sizeof(BOOL) ) != 0 )//PATCHED!
	{
		LOG("FTPServer",  "Failed to set socket to 5802, error");
		return 0;
	}

	if( setsockopt(server, SOL_SOCKET, 0x5801, (PCSTR)&bBroadcast, sizeof(BOOL) ) != 0 )//PATCHED!
	{
		LOG("FTPServer",  "Failed to set socket to 5801, error");
		return 0;
	}

	if ( bind( server, (const sockaddr*)&local, sizeof(local) ) == SOCKET_ERROR )
	{
		int Error = WSAGetLastError();
		LOG("FTPServer", "bind error %d",Error);
		return 0; 
	}

	while ( listen( server, SOMAXCONN ) != SOCKET_ERROR )
	{
		if (Exit) { ExitThread(0); }
		if(SETTINGS::getInstance().getFtpServerOn() == false ||
			Connected == FALSE) {
			Sleep(500);
			continue;
		}

		SOCKET client;
		int length;

		length = sizeof(local);
		//LOG("FTPServer", "Trying accept");
		client = accept( server, (sockaddr*)&local, &length );

		CFTPServerConn* conn = new CFTPServerConn();
		conn->CommandSocket = client;
		LOG("FTPServer","New Connection, XFERPORT : %d",XferPort);
		LOG("FTPServer","Xbox Ip : %s",xboxip.c_str());
		conn->XferPort = XferPort;
		conn->xboxip = xboxip;
		LOG("FTPServer","Xbox Ip : %s",conn->xboxip.c_str());
        PortsUsed[XferPort] = 1;

		Conns.push_back(conn);

		conn->CreateThread(CPU2_THREAD_1);
		SetThreadPriority(conn->hThread,THREAD_PRIORITY_HIGHEST);

        // Cycle through port numbers.
        XferPort = XferPortStart;
        for(;XferPort < XferPortRange;XferPort++){
            // Find an unused port numbeer 
            // This code only relevant if a port range was specified.
            if (!PortsUsed[XferPort & 255]) break;
        }
	}

	return 0;
}