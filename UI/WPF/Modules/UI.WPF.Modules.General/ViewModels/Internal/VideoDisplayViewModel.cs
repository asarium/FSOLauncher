#region Usings

using System;
using SDLGlue;

#endregion

namespace UI.WPF.Modules.General.ViewModels.Internal
{
    public class VideoDisplayViewModel
    {
        public VideoDisplayViewModel(SDLDisplayMode mode)
        {
            Width = mode.Width;
            Height = mode.Height;

            AspectRatio = ComputeAspectRatio(Width, Height);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Tuple<int, int> AspectRatio { get; private set; }

        private static int ComputeGcd(int a, int b)
        {
            while (b != 0)
            {
                if (a > b)
                {
                    a = a - b;
                }
                else
                {
                    b = b - a;
                }
            }
            return a;
        }

        public static Tuple<int, int> ComputeAspectRatio(int width, int height)
        {
            var gcd = ComputeGcd(width, height);

            width /= gcd;
            height /= gcd;

            // Special case for 16:10
            if (width == 8 && height == 5)
            {
                width = 16;
                height = 10;
            }

            return new Tuple<int, int>(width, height);
        }
    }
}
