﻿using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using FSOManagement.Annotations;
using ReactiveUI;

namespace FSOManagement.Interfaces.Mod
{
    public interface IModManager
    {
        [NotNull]
        IReadOnlyReactiveList<IModification> Modifications { get; }

        [NotNull]
        Task RefreshModsAsync(CancellationToken token);
    }
}