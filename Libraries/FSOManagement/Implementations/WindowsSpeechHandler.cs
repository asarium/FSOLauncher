#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Speech.Synthesis;
using FSOManagement.Interfaces;

#endregion

namespace FSOManagement.Implementations
{
    public class WindowsSpeechHandler : ISpeechHandler
    {
        private SpeechSynthesizer _speechSynthesizer;

        public WindowsSpeechHandler()
        {
            _speechSynthesizer = new SpeechSynthesizer();
        }

        #region ISpeechHandler Members

        public IEnumerable<string> InstalledVoices
        {
            get { return _speechSynthesizer.GetInstalledVoices().Select(voice => voice.VoiceInfo.Name); }
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

        ~WindowsSpeechHandler()
        {
            Dispose();
        }

        #endregion
    }
}
