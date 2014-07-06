// Contains some stuff for the exports to use and for the main ftp process to use

// Max connecton count
#define MAX_CONN_COUNT 6
// 16 will use ~17MB of memory with max clients...
// 6 will use like ~6.5MB of memory with max clients

// The start of the data port range
#define DATA_PORT_START 50000

// The passive mode timeout (15 seconds)
#define PASSIVE_TIMEOUT (15 * 1000)

// The connection timeout (10 minutes)
#define CONNECTION_TIMEOUT (10 * 60 * 1000)

// The dll's thread
extern HANDLE hThread;

// The processor to sit on
extern DWORD dwProcessor;

// If the server is running
extern BOOL bIsRunning;

// The login info critical section
extern CRITICAL_SECTION csLoginInfo;

// The current username and password
extern char szUser[];
extern char szPass[];

// The port to run on
extern int dPort;

// The command buffer
extern char szCmd[];

// The data ports used
extern BOOL dataPorts[];

// The active drives
extern char szDrives[32][32];

// The drive count
extern DWORD dwDriveCount;

// Stops the server, returns TRUE on success
BOOL StopServ();

// Starts the server, returns TRUE on success
BOOL StartServ();

typedef VOID (*LAUNCHGAME_FUNC)( const char * path);
extern LAUNCHGAME_FUNC pLaunchGame;

// Outputs some debug info
#ifdef _DEBUG
VOID DbgPrint(const char * fmt, ...);
#else
#define DbgPrint
#endif

// Sends a message on a socket, "220 stuff" for example
BOOL SendMsg(SOCKET sock, int code, const char * fmt, ...);
BOOL SendMsgPlain(SOCKET sock, int code, const char * message);
BOOL SendLine(SOCKET sock, const char * fmt, ...);
BOOL SendLinePlain(SOCKET sock, const char * fmt);

// Command proc
typedef struct _SOCKET_LIST_ENTRY *PSOCKET_LIST_ENTRY;
typedef BOOL (*pCommandProc)(PSOCKET_LIST_ENTRY psle);

typedef enum _CONN_STATE
{
	Error = -1, // Socket will be closed soon
	Connecting = 0, // We haven't said hello yet
	WaitingForCommand, // We are waiting for instruction
	WaitingForConnection, // We are waiting for a data connection
	ProcessingData, // We are processing a data connection
} CONN_STATE;

// Linked socket list
typedef struct _SOCKET_LIST_ENTRY
{
	// The command socket
	SOCKET sock;

	// The data socket
	SOCKET sockData;

	// The passive socket to accept connections
	SOCKET sockPasv; // We can assume that if sockPasv is valid, we are listening
	
	// The connection timeout
	DWORD dwTimeout;

	// The passive mode timeout
	DWORD dwPasvTimeout;

	// The data port used
	USHORT dataPort;

	// The connection address
	SOCKADDR_IN addr;
	
	// The data connection address
	SOCKADDR_IN dataAddr;

	// If we can read/write
	BOOL bCanWrite;
	BOOL bCanRead;

	// If we can accept a passive connection
	BOOL bCanAccept;

	// The proc to run after we get a connection
	pCommandProc ConnectionProc;

	// If we can read/write to the data sock
	BOOL bDataWrite;
	BOOL bDataRead;

	// If we are logged in
	BOOL bLoggedIn;

	// The state of the connection
	CONN_STATE csState;

	// The list entry
	LIST_ENTRY leList;

	// Where we are
	CHAR szDir[MAX_PATH];

	// The current username in the connection
	CHAR szUser[32];

	// The current file that is being operated on
	HANDLE hFile;

	// The command offset
	DWORD dwBufPos;

	// The data offset
	DWORD dwDataPos;

	// The length of the data in the buffer
	DWORD dwDataLen;

	// If we are writing to the flash
	BOOL bFlash;

	// If we have just sent a rename from cmd
	BOOL bRename;

	// The rename from buffer
	CHAR szRename[0x200];

	// The command buffer
	CHAR szBuf[0x200];

	// The data buffer
	CHAR szData[1024*1024];

	// FTP SPEEDS:
	// 0x2000 = 4 MB/s
	// 0x4000 = 8 MB/s
	// 0x6000 = 9 MB/s

} SOCKET_LIST_ENTRY, *PSOCKET_LIST_ENTRY;

// List macros
#define InsertHeadList( List, Entry ) \
	(Entry)->Flink = (List)->Flink; \
	(Entry)->Blink = (List); \
	(List)->Flink->Blink = (Entry); \
	(List)->Flink = (Entry);

#define RemoveEntryList( Entry ) \
	(Entry)->Flink->Blink = (Entry)->Blink; \
	(Entry)->Blink->Flink = (Entry)->Flink; \
	(Entry)->Flink = (Entry)->Blink = (Entry)->Flink;

// Command stuff

typedef struct _COMMAND
{
	const char * szName; // The command string, "NOOP" for example
	pCommandProc Proc; // The command function
	BOOL bLoginNotRequired; // If this is true, you dont have to log in
} COMMAND, *PCOMMAND;

extern COMMAND CommandList[];
extern DWORD CommandCount;

#define CMD_PROC( Name ) BOOL Name(PSOCKET_LIST_ENTRY psle)