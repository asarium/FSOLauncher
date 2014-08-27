using System;
using FSOManagement.Annotations;
using FSOManagement.Implementations;
using FSOManagement.Interfaces;

namespace FSOManagement.Speech
{
    public class SpeechManager
    {
        private static ISpeechHandler _speechHandler;

        [NotNull]
        public static ISpeechHandler SpeechHandler
        {
            get
            {
                if (_speechHandler == null)
                {
                    if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                    {
                        _speechHandler = new WindowsSpeechHandler();
                    }

                    // FSO doesn't support TTS on other platforms yet...
                    throw new NotSupportedException();
                }

                return _speechHandler;
            }
        }
    }
}