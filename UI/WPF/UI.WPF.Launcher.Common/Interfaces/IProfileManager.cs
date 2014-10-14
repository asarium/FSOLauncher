#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Caliburn.Micro;
using FSOManagement.Annotations;
using FSOManagement.Interfaces;
using FSOManagement.Profiles;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Interfaces
{
    public interface IProfileManager : IReactiveObject
    {
        [NotNull]
        IEnumerable<IProfile> Profiles { get; }

        [CanBeNull]
        IProfile CurrentProfile { get; set; }

        [NotNull]
        IObservable<IProfile> CurrentProfileObservable { get; }

        void AddProfile([NotNull] IProfile profile);

        [NotNull]
        IProfile CreateNewProfile([NotNull] string name);
    }
}
