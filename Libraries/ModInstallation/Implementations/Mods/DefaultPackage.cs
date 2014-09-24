#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using ModInstallation.Annotations;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using ModInstallation.Util;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class DefaultPackage : PropertyChangeBase, IPackage
    {
        #region IPackage Members

        public IModification ContainingModification { get; private set; }

        public string Notes { get; private set; }

        public string Name { get; private set; }

        public PackageStatus Status { get; private set; }

        public IEnumerable<IModDependency> Dependencies { get; private set; }

        public IEnumerable<IFileInformation> Files { get; private set; }

        public IEnumerable<IEnvironmentConstraint> EnvironmentConstraints { get; private set; }

        #endregion

        [CanBeNull]
        public static DefaultPackage InitializeFromData([NotNull] IModification parent, [NotNull] Package package,
            [CanBeNull] IErrorHandler errorHandler = null)
        {
            var newInstance = new DefaultPackage
            {
                ContainingModification = parent,
                Notes = package.notes,
                Name = package.name,
                Status = package.status,
                Files = package.files.Values.Select(info => DefaultFileInformation.InitializeFromData(info, errorHandler)).ToList(),
                Dependencies = package.dependencies.Select(DefaultModDependency.InitializeFromData)
            };

            if (package.environment != null)
            {
                newInstance.EnvironmentConstraints = package.environment.Select(GetEnvironmentContraint).ToList();
            }

            return newInstance;
        }

        [NotNull]
        public static IEnvironmentConstraint GetEnvironmentContraint([NotNull] EnvironmentConstraint env)
        {
            switch (env.type)
            {
                case EnvironmentType.Cpu_feature:
                    return new CpuFeatureEnvironmentConstraint(GetFeatureType(env.value));
                case EnvironmentType.Os:
                    return new OsEnvironmentConstraint(GetOsType(env.value));
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static FeatureType GetFeatureType(ValueTypes value)
        {
            switch (value)
            {
                case ValueTypes.None:
                    return FeatureType.None;
                case ValueTypes.SSE:
                    return FeatureType.SSE;
                case ValueTypes.SSE2:
                    return FeatureType.SSE2;
                case ValueTypes.AVX:
                    return FeatureType.AVX;
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }

        private static OsType GetOsType(ValueTypes value)
        {
            switch (value)
            {
                case ValueTypes.Windows:
                    return OsType.Windows;
                case ValueTypes.Linux:
                    return OsType.Linux;
                case ValueTypes.Macos:
                    return OsType.Macos;
                default:
                    throw new ArgumentOutOfRangeException("value");
            }
        }
    }
}
