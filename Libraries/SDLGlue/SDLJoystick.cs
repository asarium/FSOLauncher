#region Usings

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

#endregion

namespace SDLGlue
{
    public enum SDLJoystickEventType
    {
        Added,

        Removed
    }

    public class SDLJoystickEventArgs
    {
        internal SDLJoystickEventArgs(SDLJoystickEventType type, SDLJoystick joystick)
        {
            Type = type;
            Joystick = joystick;
        }

        public SDLJoystickEventType Type { get; private set; }

        public SDLJoystick Joystick { get; private set; }
    }

    public class SDLJoystick : IDisposable
    {
        #region Instance members

        private const int GUID_BUFFER_LENGTH = 33;

        private static List<SDLJoystick> _joysticks;

        private readonly IntPtr _joystick;

        private bool _disposed = false;

        private SDLJoystick(IntPtr joystick)
        {
            _joystick = joystick;

            InstanceID = SDL.SDL_JoystickInstanceID(_joystick);
        }

        public string Name
        {
            get
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("Joystick was disposed!");
                }

                return SDL.SDL_JoystickName(_joystick);
            }
        }

        public string GUID
        {
            get
            {
                if (_disposed)
                {
                    throw new InvalidOperationException("Joystick was disposed!");
                }

                var sdlJoystickGetGuid = SDL.SDL_JoystickGetGUID(_joystick);

                var guidBytes = new byte[GUID_BUFFER_LENGTH];
                SDL.SDL_JoystickGetGUIDString(sdlJoystickGetGuid, guidBytes, guidBytes.Length);

                // Remove the last character as that is a 0 character
                return Encoding.ASCII.GetString(guidBytes).Substring(0, GUID_BUFFER_LENGTH - 1).ToUpper(CultureInfo.InvariantCulture);
            }
        }

        public int InstanceID { get; private set; }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            SDL.SDL_JoystickClose(_joystick);
            _disposed = true;
        }

        public override int GetHashCode()
        {
            return _joystick.GetHashCode();
        }

        private bool Equals(SDLJoystick other)
        {
            return other.InstanceID == InstanceID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return Equals((SDLJoystick) obj);
        }

        ~SDLJoystick()
        {
            Dispose();
        }

        #endregion

        #region Static members

        #region Delegates

        public delegate void JoystickEventHandler(SDLJoystickEventArgs args);

        #endregion

        public static bool operator ==(SDLJoystick left, SDLJoystick right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(SDLJoystick left, SDLJoystick right)
        {
            return !Equals(left, right);
        }

        private static void ProcessJoystickEvents(SDL.SDL_Event e)
        {
            switch (e.type)
            {
                case SDL.SDL_EventType.SDL_JOYDEVICEADDED:
                    AddJoystick(e.jdevice.which);
                    break;
                case SDL.SDL_EventType.SDL_JOYDEVICEREMOVED:
                    RemoveJoystick(e.jdevice.which);
                    break;
                    // Ignore all other event types
            }
        }

        private static void AddJoystick(int index)
        {
            var joystick = new SDLJoystick(SDL.SDL_JoystickOpen(index));

            if (_joysticks.Any(stick => stick.InstanceID == joystick.InstanceID))
            {
                // We already know this joystick
                return;
            }

            _joysticks.Add(joystick);

            OnJoystickEvent(new SDLJoystickEventArgs(SDLJoystickEventType.Added, joystick));
        }

        private static void RemoveJoystick(int instanceID)
        {
            var removeList = _joysticks.Where(sdlJoystick => sdlJoystick.InstanceID == instanceID).ToList();

            foreach (var sdlJoystick in removeList)
            {
                OnJoystickEvent(new SDLJoystickEventArgs(SDLJoystickEventType.Removed, sdlJoystick));

                sdlJoystick.Dispose();
                _joysticks.Remove(sdlJoystick);
            }
        }

        public static void Init()
        {
            SDL.SDL_InitSubSystem(SDL.SDL_INIT_JOYSTICK);
            SDLLibrary.OnSDLEvent += ProcessJoystickEvents;

            _joysticks = new List<SDLJoystick>();

            var numJoysticks = SDL.SDL_NumJoysticks();
            for (var i = 0; i < numJoysticks; ++i)
            {
                AddJoystick(i);
            }
        }

        public static void Quit()
        {
            foreach (var sdlJoystick in _joysticks)
            {
                sdlJoystick.Dispose();
            }

            _joysticks.Clear();
            _joysticks = null;

            SDLLibrary.OnSDLEvent -= ProcessJoystickEvents;
            SDL.SDL_QuitSubSystem(SDL.SDL_INIT_JOYSTICK);
        }

        public static event JoystickEventHandler JoystickEvent;

        private static void OnJoystickEvent(SDLJoystickEventArgs args)
        {
            var handler = JoystickEvent;
            if (handler != null)
            {
                handler(args);
            }
        }

        public static IEnumerable<SDLJoystick> GetJoysticks()
        {
            return _joysticks;
        }

        #endregion
    }
}
