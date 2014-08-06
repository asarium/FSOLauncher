using System.Data.Odbc;

namespace SDLGlue
{
    public static class SDLVideo
    {
        public static void Init()
        {
            if (SDL.SDL_WasInit(SDL.SDL_INIT_VIDEO) != 0)
                return;

            SDL.SDL_Init(SDL.SDL_INIT_VIDEO);
        }

        public static void Quit()
        {
            if (SDL.SDL_WasInit(SDL.SDL_INIT_VIDEO) == 0)
                return;

            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_VIDEO);
        }
    }
}