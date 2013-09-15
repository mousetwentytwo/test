using Neurotoxin.Contour.Presentation.Infrastructure;
using Neurotoxin.Contour.Presentation.Infrastructure.Constants;

namespace Neurotoxin.Contour.Shell.Constants
{
    public static class ModuleLoadInfoCollection
    {
        public static ModuleLoadInfo ProfileEditor = new ModuleLoadInfo
        {
            ModuleName = Modules.ProfileEditor,
            Title = "Profile Editor",
            LoadCommand = LoadCommand.Load,
            Singleton = true
        };

        public static ModuleLoadInfo HexViewer = new ModuleLoadInfo
        {
            ModuleName = Modules.HexViewer,
            Title = "Hex Viewer",
            LoadCommand = LoadCommand.Load,
            Singleton = true
        };

        public static ModuleLoadInfo FtpBrowser = new ModuleLoadInfo
        {
            ModuleName = Modules.FtpBrowser,
            Title = "Ftp Client",
            LoadCommand = LoadCommand.Load,
            Singleton = true
        };

    }
}