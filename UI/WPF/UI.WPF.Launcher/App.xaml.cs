#region Usings

using System;
using System.Collections.Generic;
using System.Windows;
using Caliburn.Micro;
using Microsoft.Shell;
using Splat;
using UI.WPF.Launcher.Common.Classes;

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
            Locator.Current.GetService<IEventAggregator>().PublishOnUIThreadAsync(new InstanceLaunchedMessage(args));

            return true;
        }

        #endregion

        [STAThread]
        public static void Main()
        {
            // This application needs to be single instance as 
            //     1) The SDL DLL can not be used by two instances
            //     2) Later launching a second instance with commandline arguments should change the state of the main application
            if (SingleInstance<App>.InitializeAsFirstInstance(ApplicationIdentifier))
            {
                var application = new App();
                application.InitializeComponent();
                application.Run();

                // Allow single instance code to perform cleanup operations
                SingleInstance<App>.Cleanup();
            }
        }
    }
}
