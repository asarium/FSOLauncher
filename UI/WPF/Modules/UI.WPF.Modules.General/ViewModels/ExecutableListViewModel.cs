#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel.Composition;
using System.Linq;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;
using UI.WPF.Launcher.Common.Interfaces;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    internal class ExecutableComparer : IEqualityComparer<Executable>
    {
        #region IEqualityComparer<Executable> Members

        public bool Equals(Executable x, Executable y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            if (x == null)
            {
                return false;
            }

            if (y == null)
            {
                return false;
            }

            // This checks everything except the exe mode and the full path
            if (!Equals(x.AdditionalTags, y.AdditionalTags))
            {
                return false;
            }

            if (x.FeatureSet != y.FeatureSet)
            {
                return false;
            }

            if (x.Major != y.Major)
            {
                return false;
            }

            if (x.Minor != y.Minor)
            {
                return false;
            }

            if (x.Release != y.Release)
            {
                return false;
            }

            if (x.Revision != y.Revision)
            {
                return false;
            }

            if (x.Type != y.Type)
            {
                return false;
            }

            return true;
        }

        public int GetHashCode(Executable obj)
        {
            unchecked
            {
                var hashCode = (obj.AdditionalTags != null ? obj.AdditionalTags.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) obj.Type;
                hashCode = (hashCode * 397) ^ (int) obj.FeatureSet;
                hashCode = (hashCode * 397) ^ obj.Major;
                hashCode = (hashCode * 397) ^ obj.Minor;
                hashCode = (hashCode * 397) ^ obj.Release;
                hashCode = (hashCode * 397) ^ obj.Revision;
                return hashCode;
            }
        }

        #endregion
    }

    [Export(typeof(ExecutableListViewModel))]
    public class ExecutableListViewModel : PropertyChangedBase
    {
        private static readonly ExecutableComparer GroupingComparer = new ExecutableComparer();

        private BindableCollection<ExecutableViewModel> _executables;

        private ExecutableViewModel _selectedExecutableViewModel;

        private TotalConversion _selectedTc;

        private BindableCollection<Executable> _selectedTcExecutables;

        [ImportingConstructor]
        public ExecutableListViewModel(IProfileManager profileManager)
        {
            profileManager.WhenAnyValue(x => x.CurrentProfile.SelectedTotalConversion).BindTo(this, x => x.SelectedTc);
            this.WhenAnyValue(x => x.SelectedTc.ExecutableManager.Executables).BindTo(this, x => x.SelectedTcExecutables);
            this.WhenAny(x => x.SelectedTcExecutables, val => CreateExecutableCollection(val.Value)).BindTo(this, x => x.Executables);

            var selectedExecutable = profileManager.CurrentProfile.SelectedExecutable;
            var viewModel = FindViewModelFor(selectedExecutable, false);

            if (viewModel != null)
            {
                viewModel.SelectExecutable(selectedExecutable);

                SelectedExecutableViewModel = viewModel;
            }

            this.WhenAnyValue(x => x.SelectedExecutableViewModel.SelectedExecutable).BindTo(profileManager, x => x.CurrentProfile.SelectedExecutable);
        }

        public TotalConversion SelectedTc
        {
            get { return _selectedTc; }
            private set
            {
                if (Equals(value, _selectedTc))
                {
                    return;
                }

                if (_selectedTc != null)
                {
                    _selectedTc.ExecutableManager.StopFileSystemWatcher();
                }

                _selectedTc = value;

                if (_selectedTc != null)
                {
                    _selectedTc.ExecutableManager.StartFileSystemWatcher();
                }

                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<Executable> SelectedTcExecutables
        {
            get { return _selectedTcExecutables; }
            private set
            {
                if (Equals(value, _selectedTcExecutables))
                {
                    return;
                }
                if (_selectedTcExecutables != null)
                {
                    _selectedTcExecutables.CollectionChanged -= SelectedTcExecutablesChanged;
                }

                _selectedTcExecutables = value;

                if (_selectedTcExecutables != null)
                {
                    _selectedTcExecutables.CollectionChanged += SelectedTcExecutablesChanged;
                }

                NotifyOfPropertyChange();
            }
        }

        public ExecutableViewModel SelectedExecutableViewModel
        {
            get { return _selectedExecutableViewModel; }
            set
            {
                if (Equals(value, _selectedExecutableViewModel))
                {
                    return;
                }
                _selectedExecutableViewModel = value;
                NotifyOfPropertyChange();
            }
        }

        public BindableCollection<ExecutableViewModel> Executables
        {
            get { return _executables; }
            set
            {
                if (Equals(value, _executables))
                {
                    return;
                }
                _executables = value;

                if (_executables != null && _executables.Any())
                {
                    SelectedExecutableViewModel = _executables.First();
                }

                NotifyOfPropertyChange();
            }
        }

        private static BindableCollection<ExecutableViewModel> CreateExecutableCollection(IEnumerable<Executable> value)
        {
            if (value == null)
            {
                return new BindableCollection<ExecutableViewModel>();
            }

            var grouped = value.GroupBy(x => x, GroupingComparer);
            var viewModels = grouped.Select(CreateViewModelFromGroup);

            return new BindableCollection<ExecutableViewModel>(viewModels);
        }

        private void ExecutableAdded(Executable exe)
        {
            FindViewModelFor(exe).SetExecutable(exe);
        }

        private void ExecutableRemoved(Executable exe)
        {
            var viewModel = FindViewModelFor(exe);
            var currentViewModel = ReferenceEquals(viewModel, SelectedExecutableViewModel);

            switch (exe.Mode)
            {
                case ExecutableMode.Release:
                    viewModel.Release = null;
                    viewModel.ReleaseSelected = false;
                    break;
                case ExecutableMode.Debug:
                    viewModel.Debug = null;
                    viewModel.ReleaseSelected = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (viewModel.Debug != null || viewModel.Release != null)
            {
                return;
            }

            // No exeuctables, remove from the list
            var index = Executables.IndexOf(viewModel);
            Executables.RemoveAt(index);

            if (!currentViewModel || !Executables.Any())
            {
                return;
            }

            // Current executable removed
            index = Math.Max(0, Math.Min(index, Executables.Count - 1));
            SelectedExecutableViewModel = Executables[index];
        }

        private void SelectedTcExecutablesChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            switch (eventArgs.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    var newItems = eventArgs.NewItems.Cast<Executable>();
                    foreach (var newItem in newItems)
                    {
                        ExecutableAdded(newItem);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Remove:
                {
                    var removedItems = eventArgs.OldItems.Cast<Executable>();
                    foreach (var removedItem in removedItems)
                    {
                        ExecutableRemoved(removedItem);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Replace:
                {
                    var removedItems = eventArgs.OldItems.Cast<Executable>();
                    foreach (var removedItem in removedItems)
                    {
                        ExecutableRemoved(removedItem);
                    }

                    var newItems = eventArgs.NewItems.Cast<Executable>();
                    foreach (var newItem in newItems)
                    {
                        ExecutableAdded(newItem);
                    }
                    break;
                }
                case NotifyCollectionChangedAction.Move:
                    // The location of an element doesn't matter
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _executables.Clear();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private ExecutableViewModel FindViewModelFor(Executable executable, bool createIfNotExists = true)
        {
            foreach (var executableViewModel in _executables)
            {
                if (GroupingComparer.Equals(executableViewModel.Debug, executable))
                {
                    return executableViewModel;
                }

                if (GroupingComparer.Equals(executableViewModel.Release, executable))
                {
                    return executableViewModel;
                }
            }

            if (!createIfNotExists)
            {
                return null;
            }

            var viewModel = new ExecutableViewModel();
            _executables.Add(viewModel);

            return viewModel;
        }

        private static ExecutableViewModel CreateViewModelFromGroup(IGrouping<Executable, Executable> grouping)
        {
            var viewModel = new ExecutableViewModel();

            foreach (var executable in grouping)
            {
                switch (executable.Mode)
                {
                    case ExecutableMode.Debug:
                        viewModel.Debug = executable;
                        break;
                    case ExecutableMode.Release:
                        viewModel.Release = executable;
                        break;
                }

                if (viewModel.HasBothVersions)
                {
                    // If we have everything, discarad the rest if there is any
                    break;
                }
            }

            return viewModel;
        }
    }
}
