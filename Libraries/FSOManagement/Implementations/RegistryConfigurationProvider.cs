#region Usings

using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Interfaces;
using FSOManagement.Profiles.Keys;
using FSOManagement.Util;
using Microsoft.Win32;
using SDLGlue;

#endregion

namespace FSOManagement.Implementations
{
    public class RegistryConfigurationProvider : IConfigurationProvider
    {
        private const string VideoCardPattern = @"OGL -\((\d*)x(\d*)\)x\d* bit";

        private const string RegistryKeyName = @"{0}_Classes\VirtualStore\Machine\Software\Wow6432Node\Volition\Freespace2";

        private static readonly string UserSid = ClsLookupAccountName.GetUserSid();

        private static readonly Regex VideoCardRegex = new Regex(VideoCardPattern);

        #region IConfigurationProvider Members

        public Task PushConfigurationAsync(IProfile profile, CancellationToken token)
        {
            return Task.Run(() => WriteRegistry(profile, token), token);
        }

        public Task PullConfigurationAsync(IProfile profile, CancellationToken token)
        {
            return Task.Run(() => ReadRegistry(profile, token), token);
        }

        #endregion

        private static void ReadRegistry(IProfile profile, CancellationToken token)
        {
            var registryPath = string.Format(RegistryKeyName, UserSid);

            using (var hkeyCurrentUser = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var fsKey = hkeyCurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadSubTree))
                {
                    if (fsKey == null)
                    {
                        return;
                    }

                    var videoSettings = fsKey.GetValue(ConfigurationKeyNames.VideoCard) as string;

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

                    var joystickGuid = fsKey.GetValue(ConfigurationKeyNames.CurrentJoystickGuid) as string;

                    if (joystickGuid == null)
                    {
                        var joystickIndex = fsKey.GetValue(ConfigurationKeyNames.CurrentJoystick);

                        if (joystickIndex is int)
                        {
                            var joysticks = SDLJoystick.GetJoysticks().Skip((int) joystickIndex);

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

                    var textureFilter = fsKey.GetValue(ConfigurationKeyNames.TextureFilter);
                    if (textureFilter is int)
                    {
                        TextureFiltering filter;
                        switch ((int) textureFilter)
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

                    using (var audioFolder = fsKey.OpenSubKey(ConfigurationKeyNames.SoundFolderName))
                    {
                        if (audioFolder != null)
                        {
                            var playbackDevice = audioFolder.GetValue(ConfigurationKeyNames.PlaybackDevice) as string;

                            if (playbackDevice != null)
                            {
                                profile.SelectedAudioDevice = playbackDevice;
                            }

                            var sampleRate = audioFolder.GetValue(ConfigurationKeyNames.SampleRate);

                            if (sampleRate is int)
                            {
                                // Funny syntax...
                                profile.SampleRate = (uint) (int) sampleRate;
                            }

                            var enableEfx = audioFolder.GetValue(ConfigurationKeyNames.EnableEFX);

                            if (enableEfx is int)
                            {
                                profile.EfxEnabled = ((int) enableEfx) != 0;
                            }
                        }
                    }
                }
            }
        }

        private static void WriteRegistry(IProfile profile, CancellationToken token)
        {
            var registryPath = string.Format(RegistryKeyName, UserSid);

            using (var hkeyCurrentUser = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var fsKey = hkeyCurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (fsKey == null)
                    {
                        return;
                    }

                    var videoString = string.Format("OGL -({0}x{1})x{2} bit", profile.ResolutionWidth, profile.ResolutionHeight, 32);
                    fsKey.SetValue(ConfigurationKeyNames.VideoCard, videoString);

                    if (profile.SelectedJoystickGuid != null)
                    {
                        fsKey.SetValue(ConfigurationKeyNames.CurrentJoystickGuid, profile.SelectedJoystickGuid);

                        var joystickIndex = SDLJoystick.GetJoysticks().IndexOf(joy => joy.GUID == profile.SelectedJoystickGuid);

                        if (joystickIndex >= 0)
                        {
                            fsKey.SetValue(ConfigurationKeyNames.CurrentJoystick, joystickIndex);
                        }
                        else
                        {
                            fsKey.DeleteValue(ConfigurationKeyNames.CurrentJoystick);
                        }
                    }
                    else
                    {
                        fsKey.DeleteValue(ConfigurationKeyNames.CurrentJoystickGuid);
                        fsKey.DeleteValue(ConfigurationKeyNames.CurrentJoystick);
                    }

                    fsKey.SetValue(ConfigurationKeyNames.TextureFilter, (uint) profile.TextureFiltering, RegistryValueKind.DWord);

                    using (var audioFolder = fsKey.CreateSubKey(ConfigurationKeyNames.SoundFolderName))
                    {
                        if (audioFolder != null)
                        {
                            if (profile.SelectedAudioDevice != null)
                            {
                                audioFolder.SetValue(ConfigurationKeyNames.PlaybackDevice, profile.SelectedAudioDevice);
                            }
                            else
                            {
                                audioFolder.DeleteValue(ConfigurationKeyNames.PlaybackDevice);
                            }

                            audioFolder.SetValue(ConfigurationKeyNames.SampleRate, (int) profile.SampleRate);

                            audioFolder.SetValue(ConfigurationKeyNames.EnableEFX, profile.EfxEnabled ? 1 : 0);
                        }
                    }
                }
            }
        }
    }
}
