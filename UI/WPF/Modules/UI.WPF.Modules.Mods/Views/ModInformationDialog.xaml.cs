#region Usings

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using FSOManagement;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Mods.ViewModels;

#endregion

namespace UI.WPF.Modules.Mods.Views
{
    /// <summary>
    ///     Interaction logic for ModInformationDialog.xaml
    /// </summary>
    public partial class ModInformationDialog : IDialogControl<bool>
    {
        private readonly TaskCompletionSource<bool> _dialogCompletionSource;

        public ModInformationDialog([NotNull] IniModViewModel iniModViewModel)
        {
            InitializeComponent();

            _dialogCompletionSource = new TaskCompletionSource<bool>();

            var acceptCommand = ReactiveCommand.Create();
            acceptCommand.Subscribe(x => _dialogCompletionSource.TrySetResult(true));

            AcceptCommand = acceptCommand;

            DataContext = iniModViewModel;

            OpenWebsiteCommand = CreateLinkCommand(iniModViewModel.ModInstance, x => x.Website);

            OpenForumCommand = CreateLinkCommand(iniModViewModel.ModInstance, x => x.Forum);

            OpenBugtrackerCommand = CreateLinkCommand(iniModViewModel.ModInstance, x => x.Bugtracker);
        }

        [NotNull]
        public ICommand AcceptCommand { get; private set; }

        [NotNull]
        public ICommand OpenWebsiteCommand { get; set; }

        [NotNull]
        public ICommand OpenForumCommand { get; set; }

        [NotNull]
        public ICommand OpenBugtrackerCommand { get; set; }

        #region IDialogControl<bool> Members

        [NotNull]
        public Task<bool> WaitForCompletionAsync()
        {
            return _dialogCompletionSource.Task;
        }

        #endregion

        [NotNull]
        private static ICommand CreateLinkCommand([NotNull] IniModification viewModel, [NotNull] Expression<Func<IniModification, string>> propertyExpression)
        {
            var observable = viewModel.WhenAny(propertyExpression, val => !string.IsNullOrEmpty(val.Value));
            var propertyAccessor = propertyExpression.Compile();

            var reactiveCommand = ReactiveCommand.Create(observable);
            reactiveCommand.Subscribe(_ =>
            {
                var value = propertyAccessor(viewModel);

                Uri temp;
                var isValid = Uri.TryCreate(value, UriKind.Absolute, out temp) &&
                              (temp.Scheme == Uri.UriSchemeHttp || temp.Scheme == Uri.UriSchemeHttps);

                if (isValid)
                {
                    Process.Start(value);
                }
            });

            return reactiveCommand;
        }
    }
}
