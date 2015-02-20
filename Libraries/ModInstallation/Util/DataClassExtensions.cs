#region Usings

using System;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;

#endregion

namespace ModInstallation.Util
{
    public static class DataClassExtensions
    {
        [NotNull]
        public static Modification ToDataClass([NotNull] this IModification mod)
        {
            return new Modification
            {
                description = mod.Description,
                id = mod.Id,
                packages = mod.Packages.Select(ToDataClass),
                title = mod.Title,
                version = mod.Version.ToString(),
                logo = mod.LogoUri == null ? null : mod.LogoUri.ToString(),
                cmdline = mod.Commandline
            };
        }

        [NotNull]
        public static Package ToDataClass([NotNull] this IPackage package)
        {
            return new Package
            {
                dependencies = package.Dependencies.Select(ToDataClass),
                name = package.Name,
                notes = package.Notes,
                status = package.Status,
                filelist = package.FileList != null ? package.FileList.Select(ToDataClass) : null,
                files = Enumerable.Empty<FileInformation>()
            };
        }

        [NotNull]
        public static FileListItem ToDataClass([NotNull] this IFileListItem item)
        {
            return new FileListItem
            {
                archive = item.Archive,
                filename = item.Filename,
                md5sum = item.Verifier.StringChecksum,
                orig_name = item.OriginalName
            };
        }

        [NotNull]
        public static Dependency ToDataClass([NotNull] this IModDependency dependency)
        {
            return new Dependency
            {
                id = dependency.ModId,
                version = GetVersionString(dependency),
                packages = dependency.PackageNames
            };
        }

        [NotNull]
        private static string GetVersionString([NotNull] this IModDependency dependency)
        {
            return string.Join(",", dependency.VersionConstraints.Select(GetConstraintString));
        }

        [NotNull]
        public static string GetConstraintString([NotNull] this IVersionConstraint constraint)
        {
            string prefix;
            switch (constraint.Type)
            {
                case ConstraintType.Any:
                    return "*";
                case ConstraintType.LessThan:
                    prefix = "<";
                    break;
                case ConstraintType.LessThanEqual:
                    prefix = "<=";
                    break;
                case ConstraintType.Equal:
                    prefix = "==";
                    break;
                case ConstraintType.GreaterThanEqual:
                    prefix = ">=";
                    break;
                case ConstraintType.GreaterThan:
                    prefix = ">";
                    break;
                case ConstraintType.NotEqual:
                    prefix = "!=";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (constraint.Version != null)
            {
                return prefix + constraint.Version;
            }

            throw new InvalidOperationException();
        }
    }
}
