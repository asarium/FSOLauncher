using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FSOManagement.Annotations;

namespace FSOManagement.Interfaces
{
    public interface IVoice
    {
        string Name { get; }

        string Description { get; }

        Task SpeakAsync(string text, int volume, int rate);
    }

    public interface ISpeechHandler : IDisposable
    {
        IEnumerable<IVoice> InstalledVoices { get; }
    }
}