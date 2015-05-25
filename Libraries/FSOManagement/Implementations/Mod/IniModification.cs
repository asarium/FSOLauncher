#region Usings

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using FSOManagement.Interfaces.Mod;
using FSOManagement.Util;
using IniParser;
using IniParser.Exceptions;
using IniParser.Model;
using Splat;

#endregion

namespace FSOManagement.Implementations.Mod
{
    public class IniModification : INotifyPropertyChanged, ILocalModification, IEnableLogger
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

        private string _modRootPath;

        [NonSerialized]
        private string _name;

        [NonSerialized]
        private string _website;

        #endregion

        public IniModification(string path)
        {
            Name = Path.GetFileName(path);
            ModRootPath = path;

            Dependencies = new KeyDataCollection();
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

        #endregion

        #region ILocalModification Members

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

        #endregion

        #region INotifyPropertyChanged Members

        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected bool Equals(IniModification other)
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
            return Equals((IniModification) obj);
        }

        public override int GetHashCode()
        {
            return ModRootPath.GetHashCode();
        }

        public static bool operator ==(IniModification left, IniModification right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IniModification left, IniModification right)
        {
            return !Equals(left, right);
        }

        public async Task ReadModIniAsync()
        {
            var iniPath = Path.Combine(ModRootPath, "mod.ini");

            if (!File.Exists(iniPath))
            {
                return;
            }

            var parser = new StreamIniDataParser();
            parser.Parser.Configuration.CommentString = "#";

            var iniData = await Task.Run(() =>
            {
                try
                {
                    return parser.ReadData(new StreamReader(new FileStream(iniPath, FileMode.Open), Encoding.UTF8));
                }
                catch (IOException e)
                {
                    this.Log().WarnException("Failed to load mod.ini ("+iniPath+")!", e);
                    return null;
                }
                catch (ParsingException e)
                {
                    this.Log().WarnException("Failed to parse mod.ini (" + iniPath + ")!", e);
                    return null;
                }
            }).ConfigureAwait(false);

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
                Dependencies = iniData["multimod"];
            }
        }

        public KeyDataCollection Dependencies { get; private set; }

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
    }
}
