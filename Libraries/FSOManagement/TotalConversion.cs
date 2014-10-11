#region Usings

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;

#endregion

namespace FSOManagement
{
    [Serializable]
    public class TotalConversion : INotifyPropertyChanged, ISerializable, IEquatable<TotalConversion>
    {
        [NonSerialized]
        private readonly ModManager _modManager;

        [NonSerialized]
        private ExecutableManager _executableManager;

        private string _name;

        private string _rootFolder;

        [UsedImplicitly]
        public TotalConversion()
        {
        }

        public TotalConversion(string name, string rootFolder)
        {
            if (!Directory.Exists(rootFolder))
            {
                throw new ArgumentException("Root folder does not exist!");
            }

            _name = name;
            _rootFolder = rootFolder;

            _executableManager = new ExecutableManager(_rootFolder);
            _modManager = new ModManager(_rootFolder);
        }

        protected TotalConversion(SerializationInfo info, StreamingContext context)
        {
            _name = info.GetString("name");
            _rootFolder = info.GetString("root");

            _executableManager = new ExecutableManager(_rootFolder);
            _modManager = new ModManager(_rootFolder);
        }

        public ModManager ModManager
        {
            get { return _modManager; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name)
                {
                    return;
                }
                _name = value;
                OnPropertyChanged();
            }
        }

        public string RootFolder
        {
            get { return _rootFolder; }
            set { _rootFolder = value; }
        }

        public virtual ExecutableManager ExecutableManager
        {
            get { return _executableManager ?? (_executableManager = new ExecutableManager(_rootFolder)); }
        }

        #region IEquatable<TotalConversion> Members

        public bool Equals(TotalConversion other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }
            if (ReferenceEquals(this, other))
            {
                return true;
            }
            return string.Equals(_rootFolder, other._rootFolder);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISerializable Members

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("name", _name);
            info.AddValue("root", _rootFolder);
        }

        #endregion

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return obj is TotalConversion && Equals((TotalConversion) obj);
        }

        public override int GetHashCode()
        {
            // _rootFolder is only changed when the object is initialized
// ReSharper disable NonReadonlyFieldInGetHashCode
            return (_rootFolder != null ? _rootFolder.GetHashCode() : 0);
// ReSharper restore NonReadonlyFieldInGetHashCode
        }

        public static bool operator ==(TotalConversion left, TotalConversion right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(TotalConversion left, TotalConversion right)
        {
            return !Equals(left, right);
        }
    }
}
