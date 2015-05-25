#region Usings

using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using FSOManagement.Annotations;
using Launcher.Shared.Startup;
using Microsoft.Shell;
using ReactiveUI;
using Splat;
using Squirrel;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Modules.Update.Services;

#endregion

namespace UI.WPF.Launcher
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application, ISingleInstanceApp
    {
        private const string ApplicationIdentifier = "FsoLauncherApplication";

        public App()
        {
            ShutdownMode = ShutdownMode.OnMainWindowClose;
        }

        #region ISingleInstanceApp Members

        bool ISingleInstanceApp.SignalExternalCommandLineArgs(IList<string> args)
        {
            Locator.Current.GetService<IMessageBus>().SendMessage(new InstanceLaunchedMessage(args));

            return true;
        }

        #endregion

        [STAThread]
        public static void Main([NotNull] string[] args)
        {
            if (!CommandlineHandler.HandleCommandLine(args))
            {
                return;
            }

            SquirrelUpdateService.UpdaterMain();

            // This application needs to be single instance as 
            //     1) The SDL DLL can not be used by two instances
            //     2) Later launching a second instance with commandline arguments should change the state of the main application
            if (SingleInstance<App>.InitializeAsFirstInstance(ApplicationIdentifier))
            {
                var application = new App();
                application.InitializeComponent();

                new LauncherBootstrapper();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
    }
}
