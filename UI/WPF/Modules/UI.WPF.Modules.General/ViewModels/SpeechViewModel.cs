#region Usings

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Speech;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    public class SpeechViewModel : ReactiveObjectBase
    {
        private IProfile _profile;

        private IVoice _selectedVoice;

        private string _testPlayString = "Press play to test this string.";

        private IEnumerable<IVoice> _voices;

        public SpeechViewModel([NotNull] IProfile profile)
        {
            Voices = SpeechManager.SpeechHandler.InstalledVoices;

            profile.WhenAny(x => x.SpeechVoiceName,
                val =>
                    SpeechManager.SpeechHandler.InstalledVoices.FirstOrDefault(voice => voice.Name == val.Value) ??
                    SpeechManager.SpeechHandler.InstalledVoices.FirstOrDefault()).BindTo(this, x => x.SelectedVoice);

            this.WhenAny(x => x.SelectedVoice, val => val.Value == null ? null : val.Value.Name).BindTo(profile, x => x.SpeechVoiceName);

            Profile = profile;

            var playStringObservable = this.WhenAny(x => x.TestPlayString, val => !string.IsNullOrEmpty(val.Value));

            var playCommand = ReactiveCommand.CreateAsyncTask(playStringObservable, async _ => await PlayStringAsync());
            PlayStringCommand = playCommand;
        }

        [NotNull]
        public ICommand PlayStringCommand { get; private set; }

        [NotNull]
        public string TestPlayString
        {
            get { return _testPlayString; }
            set { RaiseAndSetIfPropertyChanged(ref _testPlayString, value); }
        }

        [NotNull]
        public IProfile Profile
        {
            get { return _profile; }
            private set { RaiseAndSetIfPropertyChanged(ref _profile, value); }
        }

        [NotNull]
        public IEnumerable<IVoice> Voices
        {
            get { return _voices; }
            private set { RaiseAndSetIfPropertyChanged(ref _voices, value); }
        }

        [CanBeNull]
        public IVoice SelectedVoice
        {
            get { return _selectedVoice; }
            set { RaiseAndSetIfPropertyChanged(ref _selectedVoice, value); }
        }

        [NotNull]
        private async Task PlayStringAsync()
        {
            if (SelectedVoice != null)
            {
                await SelectedVoice.SpeakAsync(TestPlayString, Profile.SpeechVoiceVolume, 1);
            }
        }
    }
}
