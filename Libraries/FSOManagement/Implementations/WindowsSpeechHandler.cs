#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Implementations
{
    public class WindowsVoice : IVoice
    {
        private readonly SpeechSynthesizer _speechSynthesizer;

        private readonly InstalledVoice _voice;

        public WindowsVoice(SpeechSynthesizer speechSynthesizer, InstalledVoice voice)
        {
            _speechSynthesizer = speechSynthesizer;
            _voice = voice;
        }

        #region IVoice Members

        public string Name
        {
            get { return _voice.VoiceInfo.Name; }
        }

        public string Description
        {
            get { return _voice.VoiceInfo.Description; }
        }

        public Task SpeakAsync(string text, int volume, int rate)
        {
            _speechSynthesizer.Volume = volume;
            _speechSynthesizer.Rate = rate;

            var tcs = new TaskCompletionSource<bool>();

            var eventHandler = new EventHandler<SpeakCompletedEventArgs>((sender, args) =>
            {
                if (args.Error != null)
                {
                    tcs.TrySetException(args.Error);
                }
                else
                {
                    tcs.TrySetResult(true);
                }
            });

            _speechSynthesizer.SpeakCompleted += eventHandler;

            _speechSynthesizer.SpeakAsync(text);

            return tcs.Task;
        }

        #endregion
    }

    public class WindowsSpeechHandler : ISpeechHandler
    {
        private readonly List<WindowsVoice> _voices;

        private SpeechSynthesizer _speechSynthesizer;

        public WindowsSpeechHandler()
        {
            _speechSynthesizer = new SpeechSynthesizer();

            _voices = _speechSynthesizer.GetInstalledVoices().Select(voice => new WindowsVoice(_speechSynthesizer, voice)).ToList();
        }

        #region ISpeechHandler Members

        public IEnumerable<IVoice> InstalledVoices
        {
            get { return _voices; }
        }

        public void Dispose()
        {
            if (_speechSynthesizer != null)
            {
                _speechSynthesizer.Dispose();
                _speechSynthesizer = null;
            }

            GC.SuppressFinalize(this);
        }

        #endregion

        ~WindowsSpeechHandler()
        {
            Dispose();
        }
    }
}
