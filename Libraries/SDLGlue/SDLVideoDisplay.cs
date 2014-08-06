#region Usings

using System.Collections.Generic;

#endregion

namespace SDLGlue
{
    public sealed class SDLVideoDisplay
    {
        private SDL.SDL_Rect _bounds;

        private int _index;

        public int X
        {
            get { return _bounds.x; }
        }

        public int Y
        {
            get { return _bounds.y; }
        }

        public int Width
        {
            get { return _bounds.w; }
        }

        public int Height
        {
            get { return _bounds.h; }
        }

        public IEnumerable<SDLDisplayMode> DisplayModes
        {
            get
            {
                var numModes = SDL.SDL_GetNumDisplayModes(_index);

                for (var i = 0; i < numModes; ++i)
                {
                    SDL.SDL_DisplayMode mode;
                    SDL.SDL_GetDisplayMode(_index, i, out mode);

                    yield return new SDLDisplayMode(mode.w, mode.h, mode.refresh_rate);
                }
            }
        }

        public static IEnumerable<SDLDisplayMode> DesktopVideoModes
        {
            get
            {
                var numVideoDisplays = SDL.SDL_GetNumVideoDisplays();

                for (var i = 0; i < numVideoDisplays; ++i)
                {
                    SDL.SDL_DisplayMode mode;
                    SDL.SDL_GetDesktopDisplayMode(i, out mode);

                    yield return new SDLDisplayMode(mode.w, mode.h, mode.refresh_rate);
                }
            }
        }

        public static IEnumerable<SDLVideoDisplay> GetVideoDisplays()
        {
            var numVideoDisplays = SDL.SDL_GetNumVideoDisplays();

            for (var i = 0; i < numVideoDisplays; ++i)
            {
                var display = new SDLVideoDisplay {_index = i};

                SDL.SDL_GetDisplayBounds(i, out display._bounds);

                yield return display;
            }
        }
    }
}
