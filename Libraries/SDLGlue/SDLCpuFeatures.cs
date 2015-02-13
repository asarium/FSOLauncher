namespace SDLGlue
{
    public static class SDLCpuFeatures
    {
        public static bool HasAVX
        {
            get { return SDL.SDL_HasAVX(); }
        }

        public static bool HasSSE
        {
            get { return SDL.SDL_HasSSE(); }
        }

        public static bool HasSSE2
        {
            get { return SDL.SDL_HasSSE2(); }
        }
    }
}
