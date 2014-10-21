#region Usings

using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Runtime.CompilerServices;
using FSOManagement.Annotations;
using FSOManagement.Implementations.Mod;
using FSOManagement.Interfaces;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Profiles.DataClass;
using Splat;

#endregion

namespace FSOManagement
{
    public class TotalConversion : INotifyPropertyChanged, IEquatable<TotalConversion>, IDataModel<TcData>
    {
        private TcData _data;

        [UsedImplicitly]
        public TotalConversion()
        {
        }

        [NotNull]
        public IModManager ModManager { get; private set; }

        public string Name
        {
            get { return _data.Name; }
            set
            {
                if (value == _data.Name)
                {
                    return;
                }
                _data.Name = value;
                OnPropertyChanged();
            }
        }

        public string RootFolder
        {
            get { return _data.RootPath; }
            set { _data.RootPath = value; }
        }

        [NotNull]
        public virtual ExecutableManager ExecutableManager { get; private set; }

        #region IDataModel<TcData> Members

        public void InitializeFromData(TcData data)
        {
            if (!Directory.Exists(data.RootPath))
            {
                throw new ArgumentException("Root folder does not exist!");
            }

            _data = data;

            ModManager = new ModManager
            {
                RootFolder = RootFolder
            };

            ExecutableManager = new ExecutableManager(RootFolder);
        }

        public TcData GetData()
        {
            return _data;
        }

        #endregion

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
            return string.Equals(_data.RootPath, other._data.RootPath);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
// ReSharper disable once NonReadonlyFieldInGetHashCode
            return _data.RootPath.GetHashCode();
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
