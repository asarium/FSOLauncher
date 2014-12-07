﻿#region Usings

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading.Tasks;
using FSOManagement.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class PostInstallActions : IPostInstallActions
    {
        private readonly IEnumerable<ActionData> _actionData;

        private readonly IFileSystem _fileSystem;

        public PostInstallActions([NotNull] IFileSystem fileSystem, [NotNull] IEnumerable<ActionData> actionData)
        {
            _fileSystem = fileSystem;
            _actionData = actionData;
        }

        #region IPostInstallActions Members

        public Task ExecuteActionsAsync(string installFolder)
        {
            return Task.Run(() => ExecuteActions(installFolder));
        }

        #endregion

        private static void ApplyFileOperations([NotNull] IEnumerable<string> files, [NotNull] Action<string> op)
        {
            foreach (var file in files)
            {
                op(file);
            }
        }

        public void ExecuteActions([NotNull] string installFolder)
        {
            foreach (var actionData in _actionData)
            {
                if (actionData.type == ActionType.Mkdir)
                {
                    if (actionData.paths != null)
                    {
                        foreach (var path in actionData.paths)
                        {
                            _fileSystem.Directory.CreateDirectory(_fileSystem.Path.Combine(installFolder, path));
                        }
                    }

                    continue;
                }

                var paths = GetPaths(actionData, installFolder);
                var destPath = _fileSystem.Path.Combine(installFolder, actionData.dest ?? "");
                switch (actionData.type)
                {
                    case ActionType.Delete:
                        ApplyFileOperations(paths,
                            path =>
                            {
                                if (_fileSystem.Directory.Exists(path))
                                {
                                    _fileSystem.Directory.Delete(path, true);
                                }
                                else
                                {
                                    _fileSystem.File.Delete(path);
                                }
                            });
                        break;
                    case ActionType.Move:
                        ApplyFileOperations(paths,
                            path =>
                            {
                                if (_fileSystem.Directory.Exists(path))
                                {
                                    _fileSystem.Directory.Move(path, _fileSystem.Path.Combine(destPath, _fileSystem.Path.GetFileName(path)));
                                }
                                else
                                {
                                    _fileSystem.File.Move(path, _fileSystem.Path.Combine(destPath, _fileSystem.Path.GetFileName(path)));
                                }
                            });
                        break;
                    case ActionType.Copy:
                        ApplyFileOperations(paths,
                            path =>
                            {
                                if (_fileSystem.Directory.Exists(path))
                                {
                                    DirectoryCopy(path, _fileSystem.Path.Combine(destPath, _fileSystem.Path.GetFileName(path)), true);
                                }
                                else
                                {
                                    _fileSystem.File.Copy(path, _fileSystem.Path.Combine(destPath, _fileSystem.Path.GetFileName(path)), true);
                                }
                            });
                        break;
                }
            }
        }

        private static void DirectoryCopy([NotNull] string sourceDirName, [NotNull] string destDirName, bool copySubDirs)
        {
            var dir = new DirectoryInfo(sourceDirName);
            var dirs = dir.GetDirectories();

            // If the source directory does not exist, throw an exception.
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Source directory does not exist or could not be found: " + sourceDirName);
            }

            // If the destination directory does not exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the file contents of the directory to copy.
            var files = dir.GetFiles();

            foreach (var file in files)
            {
                // Create the path to the new copy of the file.
                var temppath = Path.Combine(destDirName, file.Name);

                // Copy the file.
                file.CopyTo(temppath, false);
            }

            // If copySubDirs is true, copy the subdirectories.
            if (copySubDirs)
            {
                foreach (var subdir in dirs)
                {
                    // Create the subdirectory.
                    var temppath = Path.Combine(destDirName, subdir.Name);

                    // Copy the subdirectories.
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        [NotNull]
        public IEnumerable<string> GetPaths([NotNull] ActionData actionData, [NotNull] string installFolder)
        {
            if (actionData.paths == null)
            {
                yield break;
            }

            foreach (var path in actionData.paths.Select(CorrectPath))
            {
                var entries = _fileSystem.Directory.GetDirectories(installFolder, path, SearchOption.TopDirectoryOnly);
                foreach (var entry in entries)
                {
                    yield return entry;
                }

                entries = _fileSystem.Directory.GetFiles(installFolder, path, SearchOption.TopDirectoryOnly);
                foreach (var entry in entries)
                {
                    yield return entry;
                }
            }
        }

        [NotNull]
        private string CorrectPath([NotNull] string path)
        {
            return path.Replace(_fileSystem.Path.AltDirectorySeparatorChar, _fileSystem.Path.DirectorySeparatorChar);
        }
    }
}