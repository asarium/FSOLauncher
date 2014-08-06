using System;

namespace SDLGlue
{
    public sealed class SDLDisplayMode
    {
        public SDLDisplayMode(int width, int height, int refreshRate)
        {
            Width = width;
            Height = height;
            RefreshRate = refreshRate;
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public int RefreshRate { get; private set; }
    }
}
