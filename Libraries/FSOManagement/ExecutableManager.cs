#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using ReactiveUI;

#endregion

namespace FSOManagement
{
    public class ExecutableManager
    {
        private readonly BindableCollection<Executable> _executables;

        private readonly string _rootPath;

        private FileSystemWatcher _exeChangedWatcher;

        protected ExecutableManager()
        {
        }

        public ExecutableManager(string rootFolder)
        {
            if (!Directory.Exists(rootFolder))
            {
                throw new ArgumentException("Root folder does not exist!");
            }

            _rootPath = rootFolder;
            _executables = new BindableCollection<Executable>(ListExecutables());
        }

        #region FileSystemWatcher Event handlers

        private void ExecutableRenamed(object sender, RenamedEventArgs renamedEventArgs)
        {
            var executable = _executables.FirstOrDefault(exe => exe.FullPath == renamedEventArgs.OldFullPath);
            if (executable == null)
            {
                return;
            }

            var index = _executables.IndexOf(executable);
            _executables.Remove(executable);
            _executables.Insert(index, new Executable(renamedEventArgs.FullPath));
        }

        private void ExecutableDeleted(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            var executable = _executables.FirstOrDefault(exe => exe.FullPath == fileSystemEventArgs.FullPath);
            if (executable == null)
            {
                return;
            }

            _executables.Remove(executable);
        }

        private void ExecutableCreated(object sender, FileSystemEventArgs fileSystemEventArgs)
        {
            _executables.Add(new Executable(fileSystemEventArgs.FullPath));
        }

        #endregion

        public virtual BindableCollection<Executable> Executables
        {
            get { return _executables; }
        }

        public IEnumerable<Executable> RefreshExecutables()
        {
            _executables.Clear();

            ListExecutables().Apply(exe => _executables.Add(exe));

            return _executables;
        }

        public virtual void StartFileSystemWatcher()
        {
            if (_exeChangedWatcher != null)
            {
                return;
            }

            // Lazily start the filesystem watcher
            // TODO: Add wildcard for other platforms
            _exeChangedWatcher = new FileSystemWatcher(_rootPath, "*.exe")
            {
                EnableRaisingEvents = true,
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName
            };

            _exeChangedWatcher.Created += ExecutableCreated;
            _exeChangedWatcher.Deleted += ExecutableDeleted;
            _exeChangedWatcher.Renamed += ExecutableRenamed;
        }

        public virtual void StopFileSystemWatcher()
        {
            if (_exeChangedWatcher == null)
                return;

            _exeChangedWatcher.Created -= ExecutableCreated;
            _exeChangedWatcher.Deleted -= ExecutableDeleted;
            _exeChangedWatcher.Renamed -= ExecutableRenamed;

            _exeChangedWatcher = null;
        }

        private IEnumerable<Executable> ListExecutables()
        {
            var fsoFilePaths = Directory.EnumerateFiles(_rootPath, Executable.GlobPattern());

            foreach (var path in fsoFilePaths)
            {
                Executable exe;
                try
                {
                    exe = new Executable(path);
                }
                catch
                {
                    // Invalid name, most likely is no FSO executable
                    continue;
                }

                yield return exe;
            }

            fsoFilePaths = Directory.GetFiles(_rootPath, Executable.GlobPattern(true));

            foreach (var path in fsoFilePaths)
            {
                Executable exe;
                try
                {
                    exe = new Executable(path);
                }
                catch
                {
                    // Invalid name, most likely is no FSO executable
                    continue;
                }

                yield return exe;
            }
        }

    }
}
