#region Usings

using System.Runtime.CompilerServices;
using FSOManagement.Annotations;
using ReactiveUI;

#endregion

namespace UI.WPF.Launcher.Common.Classes
{
    public abstract class ReactiveObjectBase : ReactiveObject
    {
        [NotifyPropertyChangedInvocator]
        protected void RaiseAndSetIfPropertyChanged<T>(ref T obj, T value, [CallerMemberName] string propertyName = null)
        {
            this.RaiseAndSetIfChanged(ref obj, value, propertyName);
        }
    }
}
