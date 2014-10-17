#region Usings

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Caliburn.Micro;
using FSOManagement;
using FSOManagement.Interfaces;
using ReactiveUI;
using UI.WPF.Modules.General.ViewModels.Internal;

#endregion

namespace UI.WPF.Modules.General.ViewModels
{
    public class ExecutableListViewModel : PropertyChangedBase
    {
        private IReactiveList<ExecutableViewModel> _executables;

        private ExecutableViewModel _selectedExecutableViewModel;

        private TotalConversion _selectedTc;

        private IReactiveList<Executable> _selectedTcExecutables;

        public ExecutableListViewModel(IProfile profile)
        {
            profile.WhenAnyValue(x => x.SelectedTotalConversion).BindTo(this, x => x.SelectedTc);
            this.WhenAnyValue(x => x.SelectedTc.ExecutableManager.Executables).BindTo(this, x => x.SelectedTcExecutables);
            this.WhenAny(x => x.SelectedTcExecutables, val => CreateExecutableCollection(val.Value)).BindTo(this, x => x.Executables);

            var selectedExecutable = profile.SelectedExecutable;
            var viewModel = FindViewModelFor(selectedExecutable, false);

            if (viewModel != null)
            {
                viewModel.SelectExecutable(selectedExecutable);

                SelectedExecutableViewModel = viewModel;
            }

            this.WhenAnyValue(x => x.SelectedExecutableViewModel.SelectedExecutable).BindTo(profile, x => x.SelectedExecutable);
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

        public IReactiveList<Executable> SelectedTcExecutables
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

        public IReactiveList<ExecutableViewModel> Executables
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

        private static IReactiveList<ExecutableViewModel> CreateExecutableCollection(IEnumerable<Executable> value)
        {
            if (value == null)
            {
                return new ReactiveList<ExecutableViewModel>();
            }

            var grouped = value.GroupBy(x => x, Executable.GroupingComparer);
            var viewModels = grouped.Select(CreateViewModelFromGroup);

            var collection = new ReactiveList<ExecutableViewModel>(viewModels);

            return collection;
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
                if (Executable.GroupingComparer.Equals(executableViewModel.Debug, executable))
                {
                    return executableViewModel;
                }

                if (Executable.GroupingComparer.Equals(executableViewModel.Release, executable))
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
