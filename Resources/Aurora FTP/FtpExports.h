// Exports for all our ftp stuff

// The plugin name
extern const char * FtpPluginName;

// Returns TRUE if the plugin is running, FALSE if it is stopped
BOOL FtpIsRunning();

// Starts the plugin, returns TRUE on success
BOOL FtpStart();

// Stops the plugin, returns TRUE on success
BOOL FtpStop();

// Sets the ftp username
VOID FtpSetUsername(const char * szUsername);

// Sets the ftp password
VOID FtpSetPassword(const char * szPassword);

// Sets the username and password in one call
VOID FtpSetLogin(const char * szUsername, const char * szPassword);

// Changes the core that the thread runs on (XSetThreadProcessor)
VOID FtpSetCore(DWORD dwCore);

// Adds a drive to the list
VOID FtpAddDrive(const char * szDrive);

// Sets up the launch game function
VOID FtpSetLaunchGame( VOID* func );

// Sets the port to run on
VOID FtpSetPort(int Port);