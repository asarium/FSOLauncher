#region Usings

using System;
using System.Collections.Generic;
using System.IO.Abstractions;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;
using Semver;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class DefaultModification : PropertyChangeBase, IModification
    {
        #region IModification Members

        public string Title { get; private set; }

        public SemVersion Version { get; private set; }

        public string Id { get; private set; }

        public IEnumerable<IPackage> Packages { get; private set; }

        public string Description { get; private set; }

        public Uri LogoUri { get; private set; }

        public IPostInstallActions PostInstallActions { get; private set; }

        #endregion

        [CanBeNull]
        public static DefaultModification InitializeFromData([NotNull] Modification mod, [CanBeNull] ErrorHandler errorHandler = null)
        {
            var newInstance = new DefaultModification
            {
                Id = mod.id,
                Title = mod.title,
                Description = mod.description
            };

            if (mod.actions != null)
            {
                newInstance.PostInstallActions = new PostInstallActions(new FileSystem(), mod.actions);
            }

            Uri logo;
            if (Uri.TryCreate(mod.logo, UriKind.Absolute, out logo))
            {
                newInstance.LogoUri = logo;
            }

            newInstance.Packages =
                mod.packages.Select(pack => DefaultPackage.InitializeFromData(newInstance, pack, errorHandler))
                    .Where(p => p.EnvironmentSatisfied())
                    .ToList();

            SemVersion version;
            if (SemVersion.TryParse(mod.version, out version))
            {
                newInstance.Version = version;
            }
            else if (errorHandler != null)
            {
                if (!errorHandler(newInstance, "Version string is no valid semantic version!"))
                {
                    return null;
                }
            }

            if (mod.actions != null)
            {
            }

            return newInstance;
        }

        #region Equality members

        public bool Equals(IModification other)
        {
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return string.Equals(Id, other.Id) && Equals(Version, other.Version);
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
            return Equals((DefaultModification) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Id.GetHashCode() * 397) ^ Version.GetHashCode();
            }
        }

        #endregion
    }
}
