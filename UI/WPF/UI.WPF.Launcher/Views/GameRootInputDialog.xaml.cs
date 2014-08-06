#region Usings

using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;

#endregion

namespace UI.WPF.Launcher.Views
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
    public partial class GameRootInputDialog : SimpleDialog, IDialogControl<RootDialogResult>
    {
        public static readonly DependencyProperty AcceptButtonTextProperty = DependencyProperty.Register("AcceptButtonText", typeof(string),
            typeof(GameRootInputDialog), new PropertyMetadata("Accept"));

        public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register("CancelButtonText", typeof(string),
            typeof(GameRootInputDialog), new PropertyMetadata("Cancel"));

        public static readonly DependencyProperty SelectedNameProperty = DependencyProperty.Register("SelectedName", typeof(string),
            typeof(GameRootInputDialog), new PropertyMetadata(""));

        public static readonly DependencyProperty SelectedPathProperty = DependencyProperty.Register("SelectedPath", typeof(string),
            typeof(GameRootInputDialog), new PropertyMetadata(""));

        private readonly TaskCompletionSource<RootDialogResult> _dialogCompletionSource;

        public GameRootInputDialog()
        {
            InitializeComponent();

            _dialogCompletionSource = new TaskCompletionSource<RootDialogResult>();

            var canAcceptObservable = this.WhenAny(x => x.SelectedPath, pathObservable => IsValidPath(pathObservable.Value));

            var acceptCommand = ReactiveCommand.Create(canAcceptObservable);
            acceptCommand.Subscribe(_ => Accept());

            AcceptCommand = acceptCommand;

            var browseCommand = ReactiveCommand.CreateAsyncTask(async x => await BrowseForRoot());
            BrowseCommand = browseCommand;

            this.ObservableForProperty(x => x.SelectedPath).Subscribe(values => SetDefaultName(values.Value));
        }

        private void SetDefaultName(string path)
        {
            if (!IsValidPath(path))
                return;

            if (!string.IsNullOrEmpty(SelectedName))
                return;

            var gameDataType = FSOUtilities.GetGameTypeFromPath(path);

            switch (gameDataType)
            {
                case GameDataType.Unknown:
                    SelectedName = "Unknown game";
                    break;
                case GameDataType.FS2:
                    SelectedName = "FreeSpace 2";
                    break;
                case GameDataType.Diaspora:
                    SelectedName = "Diaspora";
                    break;
                case GameDataType.TBP:
                    SelectedName = "The Babylon Project";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public ICommand AcceptCommand { get; private set; }

        public ICommand BrowseCommand { get; private set; }

        public string SelectedName
        {
            get { return (string) GetValue(SelectedNameProperty); }
            set { SetValue(SelectedNameProperty, value); }
        }

        public string SelectedPath
        {
            get { return (string) GetValue(SelectedPathProperty); }
            set { SetValue(SelectedPathProperty, value); }
        }

        public string AcceptButtonText
        {
            get { return (string) GetValue(AcceptButtonTextProperty); }
            set { SetValue(AcceptButtonTextProperty, value); }
        }

        public string CancelButtonText
        {
            get { return (string) GetValue(CancelButtonTextProperty); }
            set { SetValue(CancelButtonTextProperty, value); }
        }

        #region IDialogControl<RootDialogResult> Members

        public Task<RootDialogResult> WaitForCompletionAsync()
        {
            return _dialogCompletionSource.Task;
        }

        #endregion

        private async Task BrowseForRoot()
        {
            var interactionService = IoC.Get<IInteractionService>();

            var path = await interactionService.OpenDirectoryDialog("Please select a directory");

            if (path == null || !IsValidPath(path))
            {
                return;
            }

            SelectedPath = path;
        }

        private static bool IsValidPath(string path)
        {
            return Directory.Exists(path);
        }

        private void Accept()
        {
            _dialogCompletionSource.TrySetResult(new RootDialogResult(SelectedName, SelectedPath, RootDialogResult.Button.Accepted));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogCompletionSource.TrySetResult(new RootDialogResult(null, null, RootDialogResult.Button.Canceled));
        }
    }
}
