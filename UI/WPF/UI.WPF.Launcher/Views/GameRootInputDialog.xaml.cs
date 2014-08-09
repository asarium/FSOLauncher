#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using MahApps.Metro.Controls.Dialogs;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Launcher.Common.Util;
using UI.WPF.Launcher.ViewModels;

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
    public partial class GameRootInputDialog : IDialogControl<RootDialogResult>
    {
        public static readonly DependencyProperty AcceptButtonTextProperty = DependencyProperty.Register("AcceptButtonText", typeof(string),
            typeof(GameRootInputDialog), new PropertyMetadata("Accept"));

        public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register("CancelButtonText", typeof(string),
            typeof(GameRootInputDialog), new PropertyMetadata("Cancel"));

        private readonly TaskCompletionSource<RootDialogResult> _dialogCompletionSource;

        public GameRootInputDialog(IEnumerable<TotalConversion> totalConversions)
        {
            _dialogCompletionSource = new TaskCompletionSource<RootDialogResult>();
            ViewModel = new GameRootInputViewModel(totalConversions);

            var acceptCommand = ReactiveCommand.Create(ViewModel.CanAcceptObservable);
            acceptCommand.Subscribe(_ => Accept());

            AcceptCommand = acceptCommand;

            DataContext = ViewModel;

            InitializeComponent();
        }

        private GameRootInputViewModel ViewModel { get; set; }

        public ICommand AcceptCommand { get; private set; }

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

        private void Accept()
        {
            _dialogCompletionSource.TrySetResult(new RootDialogResult(ViewModel.SelectedName, ViewModel.SelectedPath, RootDialogResult.Button.Accepted));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogCompletionSource.TrySetResult(new RootDialogResult(null, null, RootDialogResult.Button.Canceled));
        }
    }
}
