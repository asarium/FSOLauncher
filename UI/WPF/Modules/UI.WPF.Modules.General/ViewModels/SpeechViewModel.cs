#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using FSOManagement.Implementations;
using FSOManagement.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    public class SpeechViewModel : ReactiveObjectBase, IDisposable
    {
        private IProfile _profile;

        private IVoice _selectedVoice;

        private ISpeechHandler _speechHandler;

        private string _testPlayString = "Press play to test this string.";

        private IEnumerable<IVoice> _voices;
        
        public SpeechViewModel(IProfile profile)
        {
            _speechHandler = new WindowsSpeechHandler();
            Voices = _speechHandler.InstalledVoices;

            profile.WhenAny(x => x.SpeechVoiceName,
                val =>
                    _speechHandler.InstalledVoices.FirstOrDefault(voice => voice.Name == val.Value) ?? _speechHandler.InstalledVoices.FirstOrDefault())
                .BindTo(this, x => x.SelectedVoice);

            this.WhenAny(x => x.SelectedVoice, val => val.Value == null ? null : val.Value.Name).BindTo(profile, x => x.SpeechVoiceName);

            Profile = profile;

            var playStringObservable = this.WhenAny(x => x.TestPlayString, val => !string.IsNullOrEmpty(val.Value));

            var playCommand = ReactiveCommand.CreateAsyncTask(playStringObservable, async _ => await PlayStringAsync());
            PlayStringCommand = playCommand;
        }

        public ICommand PlayStringCommand { get; private set; }

        public string TestPlayString
        {
            get { return _testPlayString; }
            set { RaiseAndSetIfPropertyChanged(ref _testPlayString, value); }
        }

        public IProfile Profile
        {
            get { return _profile; }
            private set { RaiseAndSetIfPropertyChanged(ref _profile, value); }
        }

        public IEnumerable<IVoice> Voices
        {
            get { return _voices; }
            private set { RaiseAndSetIfPropertyChanged(ref _voices, value); }
        }

        public IVoice SelectedVoice
        {
            get { return _selectedVoice; }
            set { RaiseAndSetIfPropertyChanged(ref _selectedVoice, value); }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_speechHandler != null)
            {
                _speechHandler.Dispose();
                _speechHandler = null;
            }
        }

        #endregion

        private async Task PlayStringAsync()
        {
            await SelectedVoice.SpeakAsync(TestPlayString, Profile.SpeechVoiceVolume, 1);
        }

        ~SpeechViewModel()
        {
            Dispose();
        }
    }
}
