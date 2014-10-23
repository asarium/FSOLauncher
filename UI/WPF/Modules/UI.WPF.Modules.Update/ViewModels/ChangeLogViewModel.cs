#region Usings

using System;
using System.Collections.Generic;
using FSOManagement.Annotations;
using UI.WPF.Launcher.Common.Classes;

#endregion

namespace UI.WPF.Modules.Update.ViewModels
{
    public class ChangeLogElement : ReactiveObjectBase
    {
        private string _content;

        private Version _version;

        [NotNull]
        public Version Version
        {
            get { return _version; }
            set { RaiseAndSetIfPropertyChanged(ref _version, value); }
        }

        [NotNull]
        public string Content
        {
            get { return _content; }
            set { RaiseAndSetIfPropertyChanged(ref _content, value); }
        }
    }

    public class ChangeLogViewModel : ReactiveObjectBase
    {
        public ChangeLogViewModel([NotNull] IEnumerable<ChangeLogElement> changeLogElements)
        {
            if (changeLogElements == null)
            {
                throw new ArgumentNullException("changeLogElements");
            }
            ChangeLogElements = changeLogElements;
        }

        [NotNull]
        public IEnumerable<ChangeLogElement> ChangeLogElements { get; private set; }
    }
}
