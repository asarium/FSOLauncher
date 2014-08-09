#region Usings

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Caliburn.Micro;
using FSOManagement;
using ReactiveUI;
using UI.WPF.Launcher.Common.Services;
using UI.WPF.Launcher.Common.Util;

#endregion

namespace UI.WPF.Launcher.ViewModels
{
    public class GameRootInputViewModel : PropertyChangedBase, IDataErrorInfo
    {
        private readonly IEnumerable<TotalConversion> _totalConversions;

        private string _selectedName;

        private string _selectedPath;

        public GameRootInputViewModel(IEnumerable<TotalConversion> totalConversions)
        {
            _totalConversions = totalConversions;

            CanAcceptObservable = this.WhenAny(x => x.SelectedPath, x => x.SelectedName,
                (pathObservable, nameObservable) => !string.IsNullOrEmpty(nameObservable.Value) && GetPathError(pathObservable.Value) == null);

            var browseCommand = ReactiveCommand.CreateAsyncTask(async x => await BrowseForRoot());
            BrowseCommand = browseCommand;

            this.ObservableForProperty(x => x.SelectedPath).Subscribe(values => SetDefaultName(values.Value));
        }

        public IObservable<bool> CanAcceptObservable { get; private set; }

        public ICommand BrowseCommand { get; private set; }

        public string SelectedName
        {
            get { return _selectedName; }
            set
            {
                if (value == _selectedName)
                {
                    return;
                }
                _selectedName = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedPath
        {
            get { return _selectedPath; }
            set
            {
                if (value == _selectedPath)
                {
                    return;
                }
                _selectedPath = value;
                NotifyOfPropertyChange();
            }
        }

        #region IDataErrorInfo Members

        public string this[string columnName]
        {
            get
            {
                if (columnName == "SelectedPath")
                {
                    return GetPathError(SelectedPath);
                }

                if (columnName == "SelectedName")
                {
                    if (string.IsNullOrEmpty(SelectedName))
                        return "You have to enter a name";
                }

                return null;
            }
        }

        public string Error
        {
            get { return string.Empty; }
        }

        #endregion

        private void SetDefaultName(string path)
        {
            if (GetPathError(path) != null)
            {
                return;
            }

            if (!string.IsNullOrEmpty(SelectedName))
            {
                return;
            }

            var gameDataType = FSOUtilities.GetGameTypeFromPath(path);

            switch (gameDataType)
            {
                case GameDataType.Unknown:
                    SelectedName = "Unknown game";
                    break;
                case GameDataType.FS2:
                    SelectedName = "FreeSpace 2";
                    break;
                case GameDataType.Diaspora:
                    SelectedName = "Diaspora";
                    break;
                case GameDataType.TBP:
                    SelectedName = "The Babylon Project";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private async Task BrowseForRoot()
        {
            var interactionService = IoC.Get<IInteractionService>();

            var path = await interactionService.OpenDirectoryDialog("Please select a directory");

            if (path == null)
            {
                return;
            }

            SelectedPath = path;
        }

        private string GetPathError(string path)
        {
            if (path == null)
            {
                return "No path entered.";
            }

            if (_totalConversions.Any(tc => tc.RootFolder.PathEquals(path)))
            {
                return "Total conversion is already kown.";
            }

            if (!Directory.Exists(path))
            {
                return "Directory doesn't exist.";
            }

            return null;
        }
    }
}
