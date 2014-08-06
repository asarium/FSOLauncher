#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;

#endregion

namespace FSOManagement
{
    public class ModManager
    {
        private readonly string _rootFolder;

        private BindableCollection<Modification> _modifications;

        public ModManager(string rootFolder)
        {
            _rootFolder = rootFolder;
        }

        public IObservableCollection<Modification> Modifications
        {
            get
            {
                if (_modifications == null)
                {
                    _modifications = new BindableCollection<Modification>(GetModifications());
                }

                return _modifications;
            }
        }

        public IObservableCollection<Modification> Refresh()
        {
            _modifications = null;

            return Modifications;
        }

        public IEnumerable<Modification> GetModifications(bool searchSubfolders = false)
        {
            if (searchSubfolders)
            {
                throw new NotImplementedException("Searching sub folder is not yet implemented!");
            }

            yield return new Modification(_rootFolder);

            var possibleDirs = Directory.EnumerateDirectories(_rootFolder);

            foreach (var possibleDir in possibleDirs.Where(possibleDir => File.Exists(Path.Combine(_rootFolder, possibleDir, "mod.ini"))))
            {
                yield return new Modification(Path.Combine(_rootFolder, possibleDir));
            }
        }
    }
}
