#region Usings

using System;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using SDLGlue;

#endregion

namespace ModInstallation.Implementations.Mods
{
    public class CpuFeatureEnvironmentConstraint : IEnvironmentConstraint
    {
        private readonly FeatureType _featureType;

        public CpuFeatureEnvironmentConstraint(FeatureType featureType)
        {
            _featureType = featureType;
        }

        #region IEnvironmentConstraint Members

        public bool EnvironmentSatisfied()
        {
            switch (_featureType)
            {
                case FeatureType.None:
                    return true;
                case FeatureType.SSE:
                    return SDLCpuFeatures.HasSSE;
                case FeatureType.SSE2:
                    return SDLCpuFeatures.HasSSE2;
                case FeatureType.AVX:
                    return SDLCpuFeatures.HasAVX;
                case FeatureType.X86_32:
                    return !Environment.Is64BitOperatingSystem;
                case FeatureType.X86_64:
                    return Environment.Is64BitOperatingSystem;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #endregion
    }
}
