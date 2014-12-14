#region Usings

using System.Collections.Generic;
using MahApps.Metro.Controls;
using UI.WPF.Launcher.Common.Classes;
using UI.WPF.Launcher.Common.Interfaces;

#endregion

namespace UI.WPF.Modules.Installation.ViewModels.Installation
{
    public class InstallationFlyoutViewModel : ReactiveObjectBase, IFlyout
    {
        private object _header;

        private bool _isOpen;

        private InstallationItemParent _itemParent;

        private Position _position;

        public InstallationFlyoutViewModel()
        {
            Header = "Package installation";
            Position = Position.Right;
        }

        public InstallationItemParent ItemParent
        {
            get { return _itemParent; }
            private set { RaiseAndSetIfPropertyChanged(ref _itemParent, value); }
        }

        public IEnumerable<InstallationItem> InstallationItems
        {
            set { ItemParent = new InstallationItemParent(null, value); }
        }

        #region Implementation of IFlyout

        public bool IsOpen
        {
            get { return _isOpen; }
            set { RaiseAndSetIfPropertyChanged(ref _isOpen, value); }
        }

        public object Header
        {
            get { return _header; }
            private set { RaiseAndSetIfPropertyChanged(ref _header, value); }
        }

        public Position Position
        {
            get { return _position; }
            private set { RaiseAndSetIfPropertyChanged(ref _position, value); }
        }

        #endregion
    }
}
