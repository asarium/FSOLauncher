#region Usings

using System.ComponentModel;
using System.Runtime.CompilerServices;
using ModInstallation.Annotations;

#endregion

namespace ModInstallation.Util
{
    public class PropertyChangeBase : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([NotNull,CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
