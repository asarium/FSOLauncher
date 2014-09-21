#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using Caliburn.Micro;
using Ookii.Dialogs.Wpf;
using SDLGlue;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Properties;
using UI.WPF.Modules.Advanced;
using UI.WPF.Modules.General;
using UI.WPF.Modules.Implementations;
using UI.WPF.Modules.Installation;
using UI.WPF.Modules.Mods;
using UI.WPF.Modules.Update;

#endregion

namespace UI.WPF.Launcher
{
    public class LauncherBootstrapper : BootstrapperBase
    {
        private CompositionContainer _container;

#if !DEBUG
        private bool _unhandledExceptionCaught;
#endif

        public LauncherBootstrapper()
        {
            Initialize();
        }

        protected override void Configure()
        {
            _container = new CompositionContainer(new AggregateCatalog(AssemblySource.Instance.Select(x => new AssemblyCatalog(x))));

            var batch = new CompositionBatch();

            batch.AddExportedValue<IEventAggregator>(new EventAggregator());
            batch.AddExportedValue(_container);

            _container.Compose(batch);
        }

        protected override void OnUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Only handle exceptions in non-debug mode so the debugger can break at the location where the exception was generated
#if !DEBUG
            // Only ever allow one window to open.
            if (_unhandledExceptionCaught)
            {
                return;
            }

            _unhandledExceptionCaught = true;

            try
            {
                var dlg = new TaskDialog
                {
                    ButtonStyle = TaskDialogButtonStyle.Standard,
                    Content =
                        "An unexpected error has occured. The application can not continue to execute in this state!\n" +
                        "See below for more information. Please report this error along with the generated informations. All relevant informations are automatically copied to your clipboard.",
                    ExpandedInformation = e.Exception.ToString(),
                    MainInstruction = "Unhandled Exception!",
                    MainIcon = TaskDialogIcon.Error,
                    WindowTitle = "Unhandled Exception!",
                    ExpandedControlText = "Show more informations",
                    VerificationText = "Save error to clipboard",
                    IsVerificationChecked = true
                };

                dlg.Buttons.Add(new TaskDialogButton(ButtonType.Close));

                dlg.ShowDialog(Application.MainWindow);

                if (dlg.IsVerificationChecked)
                {
                    Clipboard.SetText(e.Exception.ToString(), TextDataFormat.UnicodeText);
                }

                SDLVideo.Quit();
                SDLJoystick.Quit();
                SDLLibrary.Quit();

                Process.GetCurrentProcess().Kill();
            }
            catch (Exception)
            {
                // Exception in uncaught exception handle? Not good!
            }
#endif
        }

        protected override object GetInstance(Type serviceType, string key)
        {
            var contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(serviceType) : key;
            var export = _container.GetExportedValues<object>(contract).FirstOrDefault();

            if (export != null)
            {
                return export;
            }

            throw new Exception(string.Format("Could not locate any instances of contract {0}.", contract));
        }

        protected override IEnumerable<object> GetAllInstances(Type serviceType)
        {
            return _container.GetExportedValues<object>(AttributedModelServices.GetContractName(serviceType));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return new[]
            {
                // ApplicationMain assembly
                typeof(LauncherBootstrapper).Assembly,

                // Modules follow
                typeof(ImplementaionsModule).Assembly,
                typeof(GeneralTabModule).Assembly,
                typeof(AdvancedTabModule).Assembly,
                typeof(ModTabModule).Assembly,
                typeof(UpdateModule).Assembly,
                typeof(InstallationModule).Assembly
            };
        }

        protected override void BuildUp(object instance)
        {
            _container.SatisfyImportsOnce(instance);
        }

        protected override void OnStartup(object sender, StartupEventArgs e)
        {
            SDLLibrary.Init(SDLLibrary.InitMode.Joystick | SDLLibrary.InitMode.Video);
            SDLJoystick.Init();
            SDLVideo.Init();

            var timer = new DispatcherTimer {Interval = TimeSpan.FromMilliseconds(1000)};
            timer.Tick += SDLUpdate;

            timer.Start();

            DisplayRootViewFor<IShellViewModel>();
        }

        private static void SDLUpdate(object sender, EventArgs eventArgs)
        {
            SDLLibrary.ProcessEvents();
        }

        protected override void OnExit(object sender, EventArgs e)
        {
            Settings.Default.Save();

            SDLVideo.Quit();
            SDLJoystick.Quit();
            SDLLibrary.Quit();

            base.OnExit(sender, e);
        }
    }
}
