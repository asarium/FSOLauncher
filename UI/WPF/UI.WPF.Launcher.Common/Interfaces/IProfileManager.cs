#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IProfileManager : IReactiveObject
    {
        IEnumerable<IProfile> Profiles { get; }

        IProfile CurrentProfile { get; set; }

        IObservable<IProfile> CurrentProfileObservable { get; }

        void AddProfile(IProfile profile);

        IProfile CreateNewProfile(string name);
    }
}
