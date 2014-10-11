#region Usings

using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows.Input;
using FSOManagement;
using FSOManagement.Implementations.Mod;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Mods.ViewModels;

#endregion

namespace UI.WPF.Modules.Mods.Views
{
    public class RootDialogResult
    {
        #region Button enum

        public enum Button
        {
            Accepted,

            Canceled
        }

        #endregion

        public RootDialogResult(string name, string path, Button selectedButton)
        {
            Name = name;
            Path = path;
            SelectedButton = selectedButton;
        }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public Button SelectedButton { get; private set; }
    }

    /// <summary>
    ///     Interaction logic for ModInformationDialog.xaml
    /// </summary>
    public partial class ModInformationDialog : IDialogControl<bool>
    {
        private readonly TaskCompletionSource<bool> _dialogCompletionSource;

        public ModInformationDialog(ModViewModel modViewModel)
        {
            InitializeComponent();

            _dialogCompletionSource = new TaskCompletionSource<bool>();

            var acceptCommand = ReactiveCommand.Create();
            acceptCommand.Subscribe(x => _dialogCompletionSource.TrySetResult(true));

            AcceptCommand = acceptCommand;

            DataContext = modViewModel;

            OpenWebsiteCommand = CreateLinkCommand(modViewModel.Mod, x => x.Website);

            OpenForumCommand = CreateLinkCommand(modViewModel.Mod, x => x.Forum);

            OpenBugtrackerCommand = CreateLinkCommand(modViewModel.Mod, x => x.Bugtracker);
        }

        public ICommand AcceptCommand { get; private set; }

        public ICommand OpenWebsiteCommand { get; set; }

        public ICommand OpenForumCommand { get; set; }

        public ICommand OpenBugtrackerCommand { get; set; }

        #region IDialogControl<bool> Members

        public Task<bool> WaitForCompletionAsync()
        {
            return _dialogCompletionSource.Task;
        }

        #endregion

        private static ICommand CreateLinkCommand(Modification viewModel, Expression<Func<Modification, string>> propertyExpression)
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
