using CommandLine;

namespace Launcher.Shared.Startup
{
    internal class Options
    {
        [Option("install-url-handler", HelpText = "Installs the handler for the fso:// URL protocol")]
        public bool InstallUrlHandler { get; set; }
    }
}