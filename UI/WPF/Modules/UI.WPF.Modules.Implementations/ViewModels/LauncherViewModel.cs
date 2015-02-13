#region Usings

using System;
using System.ComponentModel.Composition;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Annotations;
using ModInstallation.Interfaces;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.Implementations.Implementations;

#endregion

namespace UI.WPF.Modules.Implementations.ViewModels
{
    [Export(typeof(ILauncherViewModel))]
    public class LauncherViewModel : PropertyChangedBase, ILauncherViewModel
    {
        private readonly ReactiveList<IModRepositoryViewModel> _repositories;

        private readonly ReactiveList<TotalConversion> _totalConversions;

        [ImportingConstructor]
        public LauncherViewModel([NotNull] ISettings settings, [NotNull] IRepositoryFactory factory)
        {
            _totalConversions = new ReactiveList<TotalConversion>();
            _repositories = new ReactiveList<IModRepositoryViewModel>();

            settings.SettingsLoaded.Subscribe(newSettings =>
            {
                if (newSettings == null)
                {
                    return;
                }

                using (_totalConversions.SuppressChangeNotifications())
                {
                    _totalConversions.Clear();
                    _totalConversions.AddRange(newSettings.TotalConversions);
                }

                using (_repositories.SuppressChangeNotifications())
                {
                    _repositories.Clear();

                    if (newSettings.ModRepositories == null)
                    {
                        _repositories.Add(new ModRepositoryViewModel
                        {
                            Name = "Default",
                            Repository = factory.ConstructRepository("https://fsnebula.org/repo/test.json")
                        });
                    }
                    else
                    {
                        _repositories.AddRange(newSettings.ModRepositories);
                    }
                }
            });

            _totalConversions.Changed.Subscribe(_ => settings.TotalConversions = _totalConversions);
            _repositories.Changed.Subscribe(_ => settings.ModRepositories = _repositories);
        }

        #region ILauncherViewModel Members

        public IReactiveList<TotalConversion> TotalConversions
        {
            get { return _totalConversions; }
        }

        public IReactiveList<IModRepositoryViewModel> ModRepositories
        {
            get { return _repositories; }
        }

        [Import]
        public IProfileManager ProfileManager { get; private set; }

        #endregion
    }
}
