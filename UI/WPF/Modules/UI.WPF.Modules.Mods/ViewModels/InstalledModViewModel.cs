#region Usings

using System;
using FSOManagement.Annotations;
using ModInstallation.Implementations.Management;

#endregion

namespace UI.WPF.Modules.Mods.ViewModels
{
    internal class InstalledModViewModel : ModViewModel<InstalledModification>
    {
        public InstalledModViewModel([NotNull] IObservable<string> filterObservable, [NotNull] InstalledModification mod)
            : base(mod, filterObservable)
        {
        }

        protected override bool IsVisible(string filterString)
        {
            if (string.IsNullOrEmpty(filterString))
                return true;

            return ModInstance.Modification.Title.IndexOf(filterString, StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
