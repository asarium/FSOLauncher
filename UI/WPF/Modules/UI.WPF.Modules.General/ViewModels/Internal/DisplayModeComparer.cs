using System.Collections.Generic;
using SDLGlue;

namespace UI.WPF.Modules.General.ViewModels.Internal
{
    internal class DisplayModeComparer : IEqualityComparer<SDLDisplayMode>
    {
        #region IEqualityComparer<SDLDisplayMode> Members

        public bool Equals(SDLDisplayMode a, SDLDisplayMode b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            if (b == null)
            {
                return false;
            }

            return a.Width == b.Width && a.Height == b.Height;
        }

        public int GetHashCode(SDLDisplayMode mode)
        {
            unchecked
            {
                return (mode.Width * 397) ^ mode.Height;
            }
        }

        #endregion
    }
}