#region Usings

using System;

#endregion

namespace SDLGlue
{
    public static class SDLLibrary
    {
        #region InitMode enum

        [Flags]
        public enum InitMode
        {
            Timer = 0x00000001,

            Audio = 0x00000010,

            Video = 0x00000020,

            Joystick = 0x00000200,

            Haptic = 0x00001000,

            Gamecontroller = 0x00002000,

            Everything = (Timer | Audio | Video | Joystick | Haptic | Gamecontroller)
        }

        #endregion

        internal delegate void SDLEventHandler(SDL.SDL_Event e);
        internal static event SDLEventHandler OnSDLEvent;

        public static void Init(InitMode mode)
        {
            SDL.SDL_Init((uint) mode);
        }

        public static void ProcessEvents()
        {
            SDL.SDL_Event sdlEvent;

            while (SDL.SDL_PollEvent(out sdlEvent) != 0)
            {
                // Make sure someone is listening to event
                if (OnSDLEvent == null) return;

                OnSDLEvent(sdlEvent);
            }
        }

        public static void Quit()
        {
            SDL.SDL_Quit();
        }
    }
}
