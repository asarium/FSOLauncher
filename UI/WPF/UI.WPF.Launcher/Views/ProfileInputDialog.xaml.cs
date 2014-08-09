using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using FSOManagement.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Launcher.ViewModels;

namespace UI.WPF.Launcher.Views
{
    public class ProfileDialogResult
    {
        public ProfileDialogResult()
        {
            Cancelled = true;
            ClonedProfile = null;
        }

        public ProfileDialogResult(string name, string clonedProfile)
        {
            Name = name;
            Cancelled = false;
            ClonedProfile = clonedProfile;
        }

        public string Name { get; private set; }

        public bool Cancelled { get; private set; }

        public string ClonedProfile { get; private set; }
    }
    /// <summary>
    ///     Interaction logic for ModInformationDialog.xaml
    /// </summary>
    public partial class ProfileInputDialog : IDialogControl<ProfileDialogResult>
    {
        public static readonly DependencyProperty AcceptButtonTextProperty = DependencyProperty.Register("AcceptButtonText", typeof(string),
            typeof(ProfileInputDialog), new PropertyMetadata("Accept"));

        public static readonly DependencyProperty CancelButtonTextProperty = DependencyProperty.Register("CancelButtonText", typeof(string),
            typeof(ProfileInputDialog), new PropertyMetadata("Cancel"));

        private readonly TaskCompletionSource<ProfileDialogResult> _dialogCompletionSource;

        public ProfileInputDialog(IProfileManager profileManager)
        {
            _dialogCompletionSource = new TaskCompletionSource<ProfileDialogResult>();
            ViewModel = new ProfileInputViewModel(profileManager);

            var acceptCommand = ReactiveCommand.Create(ViewModel.CanAcceptObservable);
            acceptCommand.Subscribe(_ => Accept());

            AcceptCommand = acceptCommand;

            DataContext = ViewModel;

            InitializeComponent();
        }

        private ProfileInputViewModel ViewModel { get; set; }

        public ICommand AcceptCommand { get; private set; }

        public string AcceptButtonText
        {
            get { return (string)GetValue(AcceptButtonTextProperty); }
            set { SetValue(AcceptButtonTextProperty, value); }
        }

        public string CancelButtonText
        {
            get { return (string)GetValue(CancelButtonTextProperty); }
            set { SetValue(CancelButtonTextProperty, value); }
        }

        #region IDialogControl<ProfileDialogResult> Members

        public Task<ProfileDialogResult> WaitForCompletionAsync()
        {
            return _dialogCompletionSource.Task;
        }

        #endregion

        private void Accept()
        {
            _dialogCompletionSource.TrySetResult(new ProfileDialogResult(ViewModel.ProfileName, ViewModel.CloningProfile ? ViewModel.ClonedName : null));
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _dialogCompletionSource.TrySetResult(new ProfileDialogResult());
        }
    }
}