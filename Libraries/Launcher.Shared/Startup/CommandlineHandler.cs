#region Usings

using System;
using System.Security;
using System.Threading.Tasks;
using CommandLine;
using FSOManagement.Annotations;
using FSOManagement.URLHandler.Implementations;
using FSOManagement.URLHandler.Interfaces;

#endregion

namespace Launcher.Shared.Startup
{
    public static class CommandlineHandler
    {
        static CommandlineHandler()
        {
            ProtocolInstaller = CreateProtocolInstaller();
        }

        [NotNull]
        public static IProtocolInstaller ProtocolInstaller { get; set; }

        /// <summary>
        ///     Handles the given commandline arguments
        /// </summary>
        /// <param name="args">The commandline arguments to be handles</param>
        /// <returns>true if the program should continue to start, false if it should quit.</returns>
        public static bool HandleCommandLine([NotNull] string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
            {
                // If this fails, silently fail
                return true;
            }

            if (options.InstallUrlHandler)
            {
                // Need to do some tricks here to make sure this doesn't cause a deadlock
                var task = Task.Run(async () =>
                {
                    try
                    {
                        await ProtocolInstaller.InstallHandlerAsync();
                    }
                    catch (SecurityException)
                    {
                        // Ignore any exception
                    }
                });
                task.Wait();

                return false;
            }

            return true;
        }

        [NotNull]
        private static IProtocolInstaller CreateProtocolInstaller()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new WindowsProtocolInstaller();
            }

            throw new NotImplementedException();
        }
    }
}
