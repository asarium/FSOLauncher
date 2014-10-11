#region Usings

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Util;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;

#endregion

namespace FSOManagement.Implementations.Mod
{
    [Serializable]
    public class Modification : INotifyPropertyChanged, ISerializable, IModification
    {
        #region Fields

        [NonSerialized]
        private string _bugtracker;

        [NonSerialized]
        private IModDependencies _dependencies;

        [NonSerialized]
        private string _forum;

        [NonSerialized]
        private string _image;

        [NonSerialized]
        private string _infotext;

        [NonSerialized]
        private string _name;

        [NonSerialized]
        private string _website;

        private string _modRootPath;

        #endregion

        public Modification(string path)
        {
            OnCreate(path);
        }

        protected Modification(SerializationInfo info, StreamingContext context)
        {
            _modRootPath = info.GetString("modPath");

            OnCreate(_modRootPath);
        }

        #region Properties

        public string Name
        {
            get { return _name; }
            private set
            {
                if (value == _name)
                {
                    return;
                }
                _name = value;
                OnPropertyChanged();
            }
        }

        public string FolderName
        {
            get { return Path.GetFileName(ModRootPath); }
        }

        public string Image
        {
            get { return _image; }
            private set
            {
                if (value == _image)
                {
                    return;
                }
                _image = value;
                OnPropertyChanged();
            }
        }

        public string Infotext
        {
            get { return _infotext; }
            private set
            {
                if (value == _infotext)
                {
                    return;
                }
                _infotext = value;
                OnPropertyChanged();
            }
        }

        public string Website
        {
            get { return _website; }
            private set
            {
                if (value == _website)
                {
                    return;
                }
                _website = value;
                OnPropertyChanged();
            }
        }

        public string Forum
        {
            get { return _forum; }
            private set
            {
                if (value == _forum)
                {
                    return;
                }
                _forum = value;
                OnPropertyChanged();
            }
        }

        public string Bugtracker
        {
            get { return _bugtracker; }
            private set
            {
                if (value == _bugtracker)
                {
                    return;
                }
                _bugtracker = value;
                OnPropertyChanged();
            }
        }

        public IModDependencies Dependencies
        {
            get { return _dependencies; }
            private set
            {
                if (Equals(value, _dependencies))
                {
                    return;
                }
                _dependencies = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region ISerializable Members

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("modPath", _modRootPath);
        }

        #endregion

        private void OnCreate(string modPath)
        {
            Name = Path.GetFileName(modPath);
            ModRootPath = modPath;

            Dependencies = new NoModDependencies();
        }

        protected bool Equals(Modification other)
        {
            return string.Equals(_modRootPath, other.ModRootPath);
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
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((Modification) obj);
        }

        public override int GetHashCode()
        {
            return ModRootPath.GetHashCode();
        }

        public static bool operator ==(Modification left, Modification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Modification left, Modification right)
        {
            return !Equals(left, right);
        }

        public async Task ReadModIniAsync(CancellationToken token)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            var iniPath = Path.Combine(ModRootPath, "mod.ini");

            if (!File.Exists(iniPath))
            {
                return;
            }

            var parser = new FileIniDataParser();
            parser.Parser.Configuration.CommentString = "#";

            byte[] iniContent;

            using (var stream = new FileStream(iniPath, FileMode.Open, FileAccess.Read))
            {
                iniContent = new byte[stream.Length];

                await stream.ReadAsync(iniContent, 0, iniContent.Length, token);
            }

            if (token.IsCancellationRequested)
            {
                return;
            }


            var iniData = await Task.Run(() =>
            {
                try
                {
                    return parser.ReadData(new StreamReader(new MemoryStream(iniContent), Encoding.UTF8));
                }
                catch (ParsingException)
                {
                    // Ignore the error, pretend its a mod without mod.ini
                    // TODO: Add notification
                    return null;
                }
            }, token);

            if (token.IsCancellationRequested)
            {
                return;
            }

            if (iniData == null)
            {
                return;
            }

            if (iniData.Sections.ContainsSection("launcher"))
            {
                var launcherData = iniData["launcher"];

                InitializeFromIniData(launcherData);
            }

            if (iniData.Sections.ContainsSection("multimod"))
            {
                var iniDependencies = new IniModDependencies(iniData["multimod"]);

                Dependencies = iniDependencies;
            }
        }

        private void InitializeFromIniData(KeyDataCollection data)
        {
            data.TryGetValue("modname", out _name, FolderName);
            data.TryGetValue("image255x112", out _image);
            data.TryGetValue("infotext", out _infotext);
            data.TryGetValue("website", out _website);
            data.TryGetValue("forum", out _forum);
            data.TryGetValue("mantis", out _bugtracker);

            OnPropertyChanged("Name");
            OnPropertyChanged("Image");
            OnPropertyChanged("Infotext");
            OnPropertyChanged("Website");
            OnPropertyChanged("Forum");
            OnPropertyChanged("Bugtracker");
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public string ModRootPath
        {
            get { return _modRootPath; }
            private set
            {
                if (value == _modRootPath)
                {
                    return;
                }
                _modRootPath = value;
                OnPropertyChanged();
            }
        }
    }
}
