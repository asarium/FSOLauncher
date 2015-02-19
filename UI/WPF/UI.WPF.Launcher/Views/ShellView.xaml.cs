#region Usings

using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using UI.WPF.Launcher.ViewModels;

#endregion

namespace UI.WPF.Launcher.Views
{
    /// <summary>
    ///     Interaction logic for ShellView.xaml
    /// </summary>
    public partial class ShellView
    {
        private bool _closing;

        public ShellView()
        {
            this.Events().Closing.Subscribe(args =>
            {
                if (_closing)
                {
                    return;
                }

                // Cancel the first close request
                args.Cancel = true;

                _closing = true;
                var shellView = DataContext as ShellViewModel;

                if (shellView == null)
                {
                    args.Cancel = false;
                    return;
                }

                shellView.SaveSettingsAsync().ContinueWith(task => Close(), TaskScheduler.FromCurrentSynchronizationContext());
            });

            var disp = UserError.RegisterHandler(async error =>
            {
                // Make sure everything run on the UI thread
                var dialog = await Dispatcher.InvokeAsync(() => new ErrorDialog(error));

                // This feels awful...
                await await Dispatcher.InvokeAsync(() => this.ShowMetroDialogAsync(dialog));

                await await Dispatcher.InvokeAsync(() => dialog.WaitForCompletionAsync());

                await await Dispatcher.InvokeAsync(() => this.HideMetroDialogAsync(dialog));

                return error.RecoveryOptions.Select(cmd => cmd.RecoveryResult).Where(res => res.HasValue).Select(res => res.Value).FirstOrDefault();
            });

            this.Events().Closed.Subscribe(e => disp.Dispose());

            InitializeComponent();
        }
    }
}
