#region Usings

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FSOManagement.Implementations;
using FSOManagement.Interfaces;
using FSOManagement.Util;
using SDLGlue;

#endregion

namespace FSOManagement.Profiles
{
    public class ConfigurationManager
    {
        private const string VideoCardPattern = @"OGL -\((\d*)x(\d*)\)x\d* bit";

        private static readonly Regex VideoCardRegex = new Regex(VideoCardPattern);

        private static readonly IConfigurationProvider _provider = GetProvider();

        private static IConfigurationProvider GetProvider()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
            {
                return new RegistryConfigurationProvider();
            }

            throw new NotImplementedException();
        }

        public static async Task WriteConfigurationAsync(IProfile profile)
        {
            await _provider.ReadConfigurationAsync();

            var videoString = string.Format("OGL -({0}x{1})x{2} bit", profile.ResolutionWidth, profile.ResolutionHeight, 32);
            _provider.WriteValue(ConfigurationKeyNames.VideoCard, null, videoString);

            if (profile.SelectedJoystickGuid != null)
            {
                _provider.WriteValue(ConfigurationKeyNames.CurrentJoystickGuid, null, profile.SelectedJoystickGuid);

                var joystickIndex = SDLJoystick.GetJoysticks().IndexOf(joy => joy.GUID == profile.SelectedJoystickGuid);

                if (joystickIndex >= 0)
                {
                    _provider.WriteValue(ConfigurationKeyNames.CurrentJoystick, null, joystickIndex);
                }
                else
                {
                    _provider.DeleteValue(ConfigurationKeyNames.CurrentJoystick);
                }
            }
            else
            {
                _provider.DeleteValue(ConfigurationKeyNames.CurrentJoystickGuid);
                _provider.DeleteValue(ConfigurationKeyNames.CurrentJoystick);
            }

            _provider.WriteValue(ConfigurationKeyNames.TextureFilter, null, (uint) profile.TextureFiltering);

            if (profile.SelectedAudioDevice != null)
            {
                _provider.WriteValue(ConfigurationKeyNames.PlaybackDevice, ConfigurationKeyNames.SoundFolderName, profile.SelectedAudioDevice);
            }
            else
            {
                _provider.DeleteValue(ConfigurationKeyNames.PlaybackDevice, ConfigurationKeyNames.SoundFolderName);
            }

            _provider.WriteValue(ConfigurationKeyNames.SampleRate, ConfigurationKeyNames.SoundFolderName, (int) profile.SampleRate);

            _provider.WriteBool(ConfigurationKeyNames.EnableEFX, ConfigurationKeyNames.SoundFolderName, profile.EfxEnabled);


            _provider.WriteValue(ConfigurationKeyNames.SpeechVoice, null, profile.SpeechVoiceName);
            _provider.WriteValue(ConfigurationKeyNames.SpeechVolume, null, profile.SpeechVoiceVolume);

            _provider.WriteBool(ConfigurationKeyNames.SpeechBriefings, null, profile.UseVoiceInBriefing);
            _provider.WriteBool(ConfigurationKeyNames.SpeechIngame, null, profile.UseVoiceInGame);
            _provider.WriteBool(ConfigurationKeyNames.SpeechMulti, null, profile.UseVoiceInMulti);
            _provider.WriteBool(ConfigurationKeyNames.SpeechTechroom, null, profile.UseVoiceInTechRoom);

            await _provider.WriteConfigurationAsync();
        }

        public static async Task ReadConfigurationAsync(IProfile profile)
        {
            await _provider.ReadConfigurationAsync();

            var videoSettings = _provider.Read<string>(ConfigurationKeyNames.VideoCard);

            if (videoSettings != null)
            {
                var match = VideoCardRegex.Match(videoSettings);

                if (match.Groups.Count == 3)
                {
                    try
                    {
                        var width = int.Parse(match.Groups[1].Value);
                        var height = int.Parse(match.Groups[2].Value);

                        profile.ResolutionWidth = width;
                        profile.ResolutionHeight = height;
                    }
                    catch (FormatException)
                    {
                        // Ignore exception
                    }
                }
            }

            var joystickGuid = _provider.Read<string>(ConfigurationKeyNames.CurrentJoystickGuid);

            if (joystickGuid == null)
            {
                var joystickIndex = _provider.ReadValue<int>(ConfigurationKeyNames.CurrentJoystick);

                if (joystickIndex.HasValue)
                {
                    var joysticks = SDLJoystick.GetJoysticks().Skip(joystickIndex.Value);

                    if (joysticks.Any())
                    {
                        profile.SelectedJoystickGuid = joysticks.First().GUID;
                    }
                }
            }
            else
            {
                profile.SelectedJoystickGuid = joystickGuid;
            }

            var textureFilter = _provider.ReadValue<int>(ConfigurationKeyNames.TextureFilter);
            if (textureFilter.HasValue)
            {
                TextureFiltering filter;
                switch (textureFilter.Value)
                {
                    case 0:
                        filter = TextureFiltering.Bilinear;
                        break;
                    case 1:
                        filter = TextureFiltering.Trilinear;
                        break;
                    default:
                        filter = TextureFiltering.Bilinear;
                        break;
                }

                profile.TextureFiltering = filter;
            }

            var playbackDevice = _provider.Read<string>(ConfigurationKeyNames.PlaybackDevice, ConfigurationKeyNames.SoundFolderName);

            if (playbackDevice != null)
            {
                profile.SelectedAudioDevice = playbackDevice;
            }

            var sampleRate = _provider.ReadValue<int>(ConfigurationKeyNames.SampleRate, ConfigurationKeyNames.SoundFolderName);

            if (sampleRate.HasValue)
            {
                profile.SampleRate = (uint) sampleRate.Value;
            }

            var enableEfx = _provider.ReadBool(ConfigurationKeyNames.EnableEFX, ConfigurationKeyNames.SoundFolderName);

            if (enableEfx.HasValue)
            {
                profile.EfxEnabled = enableEfx.Value;
            }

            var speechVoice = _provider.Read<string>(ConfigurationKeyNames.SpeechVoice);

            if (speechVoice != null)
            {
                profile.SpeechVoiceName = speechVoice;
            }

            var speechVolume = _provider.ReadValue<int>(ConfigurationKeyNames.SpeechVolume);
            if (speechVolume.HasValue)
            {
                profile.SpeechVoiceVolume = speechVolume.Value;
            }

            {
                var speechUse = _provider.ReadBool(ConfigurationKeyNames.SpeechBriefings);
                if (speechUse.HasValue)
                {
                    profile.UseVoiceInBriefing = speechUse.Value;
                }

                speechUse = _provider.ReadBool(ConfigurationKeyNames.SpeechIngame);
                if (speechUse.HasValue)
                {
                    profile.UseVoiceInGame = speechUse.Value;
                }

                speechUse = _provider.ReadBool(ConfigurationKeyNames.SpeechMulti);
                if (speechUse.HasValue)
                {
                    profile.UseVoiceInMulti = speechUse.Value;
                }

                speechUse = _provider.ReadBool(ConfigurationKeyNames.SpeechTechroom);
                if (speechUse.HasValue)
                {
                    profile.UseVoiceInTechRoom = speechUse.Value;
                }
            }
        }
    }
}
