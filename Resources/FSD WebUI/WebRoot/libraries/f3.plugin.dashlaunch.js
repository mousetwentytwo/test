/// <reference path="f3.plugin.common.js" />

$(document).ready(function()
{
    f3.extend('data.dashlaunchinfo').translations =
    {
        "regions":
        {
            "0x00FF": "NTSC/US", // 1
            "0x01FE": "NTSC/JAP", // 2
            "0x01FF": "NTSC/JAP", // 3
            "0x01FC": "NTSC/KOR", // 4
            "0x0101": "NTSC/HK", // 5
            "0x02FE": "PAL/EU", // 6
            "0x0201": "PAL/AUS", // 7
            "0x7FFF": "DEVKIT/ALL" // 8
        },

        "oopsie":
        {
            "webui": "No translation",
            "dashlaunch": "Sorry there is no description associated with this option (oops!)"
        },
        "opt_fil_paths":
        {
            "webui": "Path settings",
            "dashlaunch": "Path settings such as Quick Launch Buttons"
        },
        "opt_fil_behavior":
        {
            "webui": "Behavioral settings",
            "dashlaunch": "Settings that affect the behaviour of the Xbox system software"
        },
        "opt_fil_net":
        {
            "webui": "Network settings",
            "dashlaunch": "Settings that affect network"
        },
        "opt_fil_timer":
        {
            "webui": "Settings that run on timers",
            "dashlaunch": "Settings that run on timers"
        },
        "opt_fil_plugins":
        {
            "webui": "Path settings for plugins",
            "dashlaunch": "Path settings for dll plugins"
        },
        "opt_fil_extern":
        {
            "webui": "DashLaunch Installer settings",
            "dashlaunch": "Options stored in dash launch that change how this app works"
        },
        "plugin1":
        {
            "webui": "Plugin loaded by DashLaunch",
            "dashlaunch": "plugin dll loaded with DashLaunch on bootup"
        },
        "plugin2":
        {
            "webui": "Plugin loaded by DashLaunch",
            "dashlaunch": "plugin dll loaded with DashLaunch on bootup"
        },
        "plugin3":
        {
            "webui": "Plugin loaded by DashLaunch",
            "dashlaunch": "plugin dll loaded with DashLaunch on bootup"
        },
        "plugin4":
        {
            "webui": "Plugin loaded by DashLaunch",
            "dashlaunch": "plugin dll loaded with DashLaunch on bootup"
        },
        "plugin5":
        {
            "webui": "Plugin loaded by DashLaunch",
            "dashlaunch": "plugin dll loaded with DashLaunch on bootup"
        },
        "BUT_A":
        {
            "webui": "Title to load when holding A on boot",
            "dashlaunch": "Holding A button on boot or when titles are exiting will cause this to load instead"
        },
        "BUT_B":
        {
            "webui": "Title to load when holding B on boot",
            "dashlaunch": "Holding B button on boot or when titles are exiting will cause this to load instead"
        },
        "BUT_X":
        {
            "webui": "Title to load when holding X on boot",
            "dashlaunch": "Holding X button on boot or when titles are exiting will cause this to load instead"
        },
        "BUT_Y":
        {
            "webui": "Title to load when holding Y on boot",
            "dashlaunch": "Holding Y button on boot or when titles are exiting will cause this to load instead"
        },
        "Start":
        {
            "webui": "Title to load when holding Start on boot",
            "dashlaunch": "Holding Start button on boot or when titles are exiting will cause this to load instead"
        },
        "Back":
        {
            "webui": "Title to load when holding Back on boot",
            "dashlaunch": "Holding Back button on boot or when titles are exiting will cause this to load instead"
        },
        "LBump":
        {
            "webui": "Title to load when holding Left Bumber on boot",
            "dashlaunch": "Holding Left Bumper on boot or when titles are exiting will cause this to load instead"
        },
        "LThumb":
        {
            "webui": "Title to load when holding Left Thumbstick on boot",
            "dashlaunch": "Holding Left Thumbstick button down on boot or when titles are exiting will cause this to load instead"
        },
        "RThumb":
        {
            "webui": "Title to load when holding Right Thumbstick on boot",
            "dashlaunch": "Holding Right Thumbstick button down on boot or when titles are exiting will cause this to load instead"
        },
        "Default":
        {
            "webui": "Title to load by default (when no buttons are held)",
            "dashlaunch": "if set, when no button is held any time dash attempts to load this will load instead"
        },
        "Guide":
        {
            "webui": "Title to load when Xbox powered on with the Guide button",
            "dashlaunch": "If set turning console on with the remote or controller guide button will launch this (one time)"
        },
        "Power":
        {
            "webui": "Title to load when Xbox powered on with the Power button",
            "dashlaunch": "If set turning console on with the console power button will launch this (one time)"
        },
        "dumpfile":
        {
            "webui": "Filename of crash dumps (when Exception Handler is enabled)",
            "dashlaunch": "Crash dumps dump to this file if exchandler option is enabled"
        },
        "configapp":
        {
            "webui": "Title to load when the System Settings option in miniblades is used",
            "dashlaunch": "Exiting via miniblades using option System Settings will go to this program if Right Bumper is not held"
        },
        "nxemini":
        {
            "webui": "Enable launch event when pressing Y in miniblades",
            "dashlaunch": "If enabled pressing Y in miniblades while in official dash will cause a launch event"
        },
        "pingpatch":
        {
            "webui": "Remove ping limit for system link",
            "dashlaunch": "If enabled ping limit will be removed for system link play"
        },
        "contpatch":
        {
            "webui": "Patch XBLA so it does not need yaris patching",
            "dashlaunch": "if enabled (most) xbla will no longer need yaris patching"
        },
        "dvdexitdash":
        {
            "webui": "Return to official dash after eject",
            "dashlaunch": "If enabled ejecting a game or DVD video will return to official dash"
        },
        "xblaexitdash":
        {
            "webui": "Return to official dash after exeting an XBLA game through its own menu",
            "dashlaunch": "If enabled exiting a XBLA game through its own menu will return you to official dash"
        },
        "nosysexit":
        {
            "webui": "Prevent System Settings option in miniblades from launching official dash",
            "dashlaunch": "If enabled system options in miniblades will not exit to official dash"
        },
        "regionspoof":
        {
            "webui": "Enable temporary region spoofing when holding RB while launching a game",
            "dashlaunch": "If enabled holding RB during title launch will cause the region to be spoofed to the title for it's duration"
        },
        "region":
        {
            "webui": "Region used when spoofing the regioin on a game with RB",
            "dashlaunch": "The region applied when RB is held on title launch and regionspoof is enabled"
        },
        "nohud":
        {
            "webui": "Disable the miniblades",
            "dashlaunch": "When enabled the miniblades are made inaccessible"
        },
        "noupdater":
        {
            "webui": "Make XAM look for updates in $$ystemUpdate (instead of $SystemUpdate)",
            "dashlaunch": "When enabled XAM is patched to look for updates at $$ystemUpdate"
        },
        "signnotice":
        {
            "webui": "Attempt to dismiss annoying signin notice messages",
            "dashlaunch": "When enabled DashLaunch will attempt to dismiss annoying signin notice messages"
        },
        "xhttp":
        {
            "webui": "Allow insecure access with XHttp",
            "dashlaunch": "When enabled XHttp is patched in XAM to allow insecure access"
        },
        "liveblock":
        {
            "webui": "Block LIVE sites in DNS resolver",
            "dashlaunch": "When enabled DNS resolver blocks LIVE sites"
        },
        "livestrong":
        {
            "webui": "Block more domains than with liveblock (liveblock must be enabled)",
            "dashlaunch": "Has no effect when liveblock is disbled. When enabled with liveblock a more extensive list of domains are blocked than with liveblock alone"
        },
        "autoshut":
        {
            "webui": "Select 'shutdown' by default when holding Guide button (affects autooff)",
            "dashlaunch": "If enabled holding Guide button will have 'shutdown' selected, toggling affects autooff"
        },
        "autooff":
        {
            "webui": "Shutdown when holding Guide button, bypassing shutdown options (affects autoshut)",
            "dashlaunch": "If enabled holding Guide button will cause the console to shut down instead of display shutdown options, toggling affects autoshut"
        },
        "hddtimer":
        {
            "webui": "Polling frequency (in seconds) to keep alive hdd's when hddalive is enabled",
            "dashlaunch": "When hddalive is enabled this sets the frequency (in seconds) that DashLaunch will poll drives at"
        },
        "hddalive":
        {
            "webui": "Poll any USB drive with 'alive.txt' in its root at the interval set in hddtimer",
            "dashlaunch": "If enabled, any USB drive with the file 'alive.txt' in it's root directory will be polled at hddtimer intervals"
        },
        "temptime":
        {
            "webui": "Broadcast frequency (in seconds) for temperature info when tempbcast is enabled",
            "dashlaunch": "Frequency (in seconds) that temperature info is broadcast when enabled"
        },
        "tempport":
        {
            "webui": "Broadcast port for temperature info when tempbcast is enabled",
            "dashlaunch": "Port that temperature will be broadcast on when enabled"
        },
        "tempbcast":
        {
            "webui": "Enable broadcasting of temperature info when a network connection is available",
            "dashlaunch": "When enabled and a network connection is provided, temperature data will be broadcast at temptime intervals to tempport"
        },
        "exchandler":
        {
            "webui": "Exit on non-fatal crashes instead of crashing the console",
            "dashlaunch": "When enabled non-fatal crashes will exit instead of crash the console"
        },
        "fatalfreeze":
        {
            "webui": "Freeze the console on crash events",
            "dashlaunch": "If enabled crash events will freeze the console"
        },
        "fatalreboot":
        {
            "webui": "Hard reboot on fatal crashes (fatalfreeze must be disabled)",
            "dashlaunch": "When enabled console will hard reboot on fatal crashes - does nothing if fatalfreeze is enabled"
        },
        "safereboot":
        {
            "webui": "Soft reboot on fatal crashes (not jtag friendly)",
            "dashlaunch": "When enabled console will soft reboot on fatal crashes (not jtag friendly)"
        },
        "debugout":
        {
            "webui": "Reroute debug prints to UART",
            "dashlaunch": "If enabled debug prints are rerouted to UART"
        },
        "passlaunch":
        {
            "webui": "Don't clean up launch data",
            "dashlaunch": "If enabled DashLaunch will not clean up launchdata"
        },
        "sockpatch":
        {
            "webui": "Patch sockets",
            "dashlaunch": "Don't enable this if you don't know what it does."
        },
        "fakelive":
        {
            "webui": "Fake live",
            "dashlaunch": "Don't enable this if you don't know what it does."
        },
        "remotenxe":
        {
            "webui": "Go to official dash when powering on Xbox with IR remote power button",
            "dashlaunch": "If enabled powering on the console with IR remote power button will go to official dash (one time)"
        },
        "nonetstore":
        {
            "webui": "Remove cloud storage from disk selection dialogs",
            "dashlaunch": "If enabled cloud storage should not show up in disk selection dialogs"
        },
        "shuttemps":
        {
            "webui": "Show snapshot of temperatures on shutdown options screen (disables autooff)",
            "dashlaunch": "If enabled a snapshot of the system temperature data will be displayed on the shutdown screen that appears when you hold guide button (enabling will disable autooff)"
        },
        "devprof":
        {
            "webui": "Prevent devkit profiles from showing as corrupt",
            "dashlaunch": "If enabled devkit profiles will not appear as corrupt"
        },
        "devlink":
        {
            "webui": "Encrypt system link data for communication with devkits",
            "dashlaunch": "If enabled system link data will be encrypted for communication with devkits"
        },
        "ftpserv":
        {
            "webui": "Enable DashLaunch installers FTP server",
            "dashlaunch": "If enabled the installer will start it's FTP server"
        },
        "ftpport":
        {
            "webui": "DashLaunch installers FTP servers port",
            "dashlaunch": "Sets the port for the installer FTP server"
        },
        "calaunch":
        {
            "webui": "Show launch menu instead of options when Dash Launch installer is started",
            "dashlaunch": "If enabled installer will start to the launch menu instead of options"
        },
        "autoswap":
        {
            "webui": "Automatic disk swapping via Dash Launch",
            "dashlaunch": "If enabled dash launch will perform automatic disk swapping"
        },
        "nohealth":
        {
            "webui": "Prevent Kinect health pseudo video from showing at game launch",
            "dashlaunch": "If enabled kinect health pseudo video at game launch will not be shown"
        },
        "nooobe":
        {
            "webui": "Don't show dash locale setup screens when dash starts if settings already exist",
            "dashlaunch": "If enabled dash locale setup screens when settings already exist will not be shown when dash starts"
        },
        "autofake":
        {
            "webui": "Automatically enable fakelive functionality only for official dash and indie game sessions",
            "dashlaunch": "If enabled fakelive functionality will automatically be enabled only for official dash and indie game sessions"
        },
        "farenheit":
        {
            "webui": "Show all temperatures in Fahrenheit instead of Celsius",
            "dashlaunch": "If enabled temps in installer and in guide (shuttemps option) will be shown in farenheit instead of celcius."
        },
        "Fakeanim":
        {
            "webui": "Title to run before any other options occur<br />(intended for video player to allow replacing bootanim)",
            "dashlaunch": "if set, this will be run as a title before any other option occurs, does not get circumvented by held buttons or default item; intended for a short video player to allow replacing bootanim"
        }
    };
    
});