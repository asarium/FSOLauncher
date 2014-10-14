#region Usings

using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
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
            InitializeComponent();

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
        }
    }
}
