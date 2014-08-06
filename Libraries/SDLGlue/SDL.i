%module SDL
%include "typemaps.i"

%{
#include "SDL_main.h"
#include "SDL.h"
#include "SDL_types.h"
#include "SDL_video.h"
#include "SDL_events.h"
#include "SDL_version.h"

#include "SDL_audio.h"
#include "SDL_mixer.h"
%}

%import "begin_code.h"
%include "SDL_main.h"
%include "SDL.h"
%include "SDL_types.h"
%include "SDL_video.h"
%include "SDL_rwops.h"
%include "SDL_version.h"
%include "SDL_error.h"
%include "SDL_keyboard.h"
%include "SDL_mouse.h"
%include "SDL_joystick.h"
%include "SDL_events.h"
%include "SDL_timer.h"


%include "SDL_audio.h"
%include "close_code.h"

typedef unsigned char Uint8;
typedef unsigned int Uint32;
typedef unsigned short Uint16;
typedef signed int Sint32;
typedef signed short Sint16;
