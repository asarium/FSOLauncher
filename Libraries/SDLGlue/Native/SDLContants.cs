using System;

namespace SDLGlue
{
    public static class SDLContants
    {
        public const UInt32 INIT_TIMER = 0x00000001;

        public const UInt32 INIT_AUDIO = 0x00000010;

        public const UInt32 INIT_VIDEO = 0x00000020; /**< SDL_INIT_VIDEO implies SDL_INIT_EVENTS */

        public const UInt32 INIT_JOYSTICK = 0x00000200; /**< SDL_INIT_JOYSTICK implies SDL_INIT_EVENTS */

        public const UInt32 INIT_HAPTIC = 0x00001000;

        public const UInt32 INIT_GAMECONTROLLER = 0x00002000; /**< SDL_INIT_GAMECONTROLLER implies SDL_INIT_JOYSTICK */

        public const UInt32 INIT_EVENTS = 0x00004000;

        public const UInt32 INIT_NOPARACHUTE = 0x00100000; /**< Don't catch fatal signals */

        public const UInt32 INIT_EVERYTHING = (INIT_TIMER | INIT_AUDIO | INIT_VIDEO | INIT_EVENTS | INIT_JOYSTICK | INIT_HAPTIC | INIT_GAMECONTROLLER);
    }
}
