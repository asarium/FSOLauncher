#region Usings

using System.Reactive;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FSOManagement.Annotations;
using ReactiveUI;
using Splat;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Launcher.Views
{
    /// <summary>
    ///     Interaction logic for ErrorDialog.xaml
    /// </summary>
    public partial class ErrorDialog : IDialogControl<Unit>, IEnableLogger
    {
        public static readonly DependencyProperty UserErrorProperty = DependencyProperty.Register("UserError",
            typeof(UserError),
            typeof(ErrorDialog),
            new PropertyMetadata(default(UserError)));

        private readonly TaskCompletionSource<Unit> _resultSource;

        public ErrorDialog([NotNull] UserError error)
        {
            _resultSource = new TaskCompletionSource<Unit>();

            DataContext = error;
            UserError = error;

            InitializeComponent();
        }

        [NotNull]
        public UserError UserError
        {
            get { return (UserError) GetValue(UserErrorProperty); }
            set { SetValue(UserErrorProperty, value); }
        }

        #region IDialogControl<ErrorResult> Members

        public Task<Unit> WaitForCompletionAsync()
        {
            return _resultSource.Task;
        }

        #endregion

        private void CommandButtonClick([NotNull] object sender, [NotNull] RoutedEventArgs e)
        {
            _resultSource.TrySetResult(Unit.Default);
        }
    }
}
