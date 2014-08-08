using System;
using System.Collections.Generic;
using FSOManagement.Annotations;

namespace FSOManagement.Interfaces
{
    public interface ISpeechHandler : IDisposable
    {
        IEnumerable<string> InstalledVoices { get; }
    }
}