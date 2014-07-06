#include <xtl.h>
#include "FtpDll.h"
#include "FtpExports.h"

// The plugin name
const char * FtpPluginName = "FtpDll";

// Returns TRUE if the plugin is running, FALSE if it is stopped
BOOL FtpIsRunning()
{
	return bIsRunning;
}

// Starts the plugin, returns TRUE on success
BOOL FtpStart()
{
	return StartServ();
}

// Stops the plugin, returns TRUE on success
BOOL FtpStop()
{
	return StopServ();
}

// Sets the ftp username
VOID FtpSetUsername(const char * szUsername)
{
	EnterCriticalSection(&csLoginInfo);
	int ucount = strlen(szUsername);
	if (ucount > 47) ucount = 47;
	memset(szUser, 0, 48);
	strncpy(szUser, szUsername, ucount);

	LeaveCriticalSection(&csLoginInfo);
}

// Sets the ftp password
VOID FtpSetPassword(const char * szPassword)
{
	EnterCriticalSection(&csLoginInfo);
	int pcount = strlen(szPassword);
	if (pcount > 47) pcount = 47;
	memset(szPass, 0, 48);
	strncpy(szPass, szPassword, pcount);

	LeaveCriticalSection(&csLoginInfo);
}

// Sets the username and password in one call
VOID FtpSetLogin(const char * szUsername, const char * szPassword)
{
	EnterCriticalSection(&csLoginInfo);
	int ucount = strlen(szUsername);
	if (ucount > 47) ucount = 47;
	int pcount = strlen(szPassword);
	if (pcount > 47) pcount = 47;
	memset(szUser, 0, 48); memset(szPass, 0, 48);
	strncpy(szUser, szUsername, ucount);
	strncpy(szPass, szPassword, pcount);

	LeaveCriticalSection(&csLoginInfo);
}

// Changes the core that the thread runs on (XSetThreadProcessor)
VOID FtpSetCore(DWORD dwCore)
{
	dwProcessor = dwCore;

	XSetThreadProcessor(hThread, dwProcessor);
}

// Adds a drive to the list
VOID FtpAddDrive(const char * szDrive)
{
	strncpy(szDrives[dwDriveCount], szDrive, 32);
	dwDriveCount++;
}

VOID FtpSetLaunchGame( VOID * func )
{
	pLaunchGame = (LAUNCHGAME_FUNC)func;
}

VOID FtpSetPort(int Port) {
	dPort = Port;
}