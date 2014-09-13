using System;
using ModInstallation.Implementations.DataClasses;
using ModInstallation.Interfaces.Mods;
using SDLGlue;

namespace ModInstallation.Implementations.Mods
{
    public class CpuFeatureEnvironmentConstraint : IEnvironmentConstraint
    {
        private readonly FeatureType _featureType;

        public CpuFeatureEnvironmentConstraint(FeatureType featureType)
        {
            _featureType = featureType;
        }

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
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}