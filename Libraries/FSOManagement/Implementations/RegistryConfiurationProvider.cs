﻿#region Usings

using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FSOManagement.Interfaces;
using FSOManagement.Util;
using Microsoft.Win32;
using SDLGlue;

#endregion

namespace FSOManagement.Implementations
{
    internal static class ClsLookupAccountName
    {
        // Using IntPtr for pSID insted of Byte[]
        [DllImport("advapi32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ConvertSidToStringSid(IntPtr pSid, out IntPtr ptrSid);

        [DllImport("kernel32.dll")]
        private static extern IntPtr LocalFree(IntPtr hMem);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool GetTokenInformation(IntPtr tokenHandle, TOKEN_INFORMATION_CLASS tokenInformationClass, IntPtr tokenInformation,
            int tokenInformationLength, out int returnLength);

        public static string GetUserSid()
        {
            var tokenInfLength = 0;

            // first call gets lenght of TokenInformation
            GetTokenInformation(WindowsIdentity.GetCurrent().Token, TOKEN_INFORMATION_CLASS.TokenUser, IntPtr.Zero, tokenInfLength, out tokenInfLength);

            var tokenInformation = Marshal.AllocHGlobal(tokenInfLength);

            var result = GetTokenInformation(WindowsIdentity.GetCurrent().Token, TOKEN_INFORMATION_CLASS.TokenUser, tokenInformation, tokenInfLength,
                out tokenInfLength);

            try
            {
                if (result)
                {
                    var tokenUser = (TOKEN_USER) Marshal.PtrToStructure(tokenInformation, typeof(TOKEN_USER));

                    IntPtr pstr;

                    var ok = ConvertSidToStringSid(tokenUser.User.Sid, out pstr);

                    if (!ok)
                    {
                        return null;
                    }

                    var sidstr = Marshal.PtrToStringAuto(pstr);
                    LocalFree(pstr);

                    return sidstr;
                }
                else
                {
                    return null;
                }
            }
            finally
            {
                Marshal.FreeHGlobal(tokenInformation);
            }
        }

        #region Nested type: SID_AND_ATTRIBUTES

        [StructLayout(LayoutKind.Sequential)]
        public struct SID_AND_ATTRIBUTES
        {
            public IntPtr Sid;

            public int Attributes;
        }

        #endregion

        #region Nested type: TOKEN_INFORMATION_CLASS

        private enum TOKEN_INFORMATION_CLASS
        {
            TokenUser = 1,

            TokenGroups,

            TokenPrivileges,

            TokenOwner,

            TokenPrimaryGroup,

            TokenDefaultDacl,

            TokenSource,

            TokenType,

            TokenImpersonationLevel,

            TokenStatistics,

            TokenRestrictedSids,

            TokenSessionId,

            TokenGroupsAndPrivileges,

            TokenSessionReference,

            TokenSandBoxInert,

            TokenAuditPolicy,

            TokenOrigin
        }

        #endregion

        #region Nested type: TOKEN_USER

        public struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }

        #endregion
    }

    public class RegistryConfiurationProvider : IConfigurationProvider
    {
        private static readonly string UserSid = ClsLookupAccountName.GetUserSid();

        private const string VideoCardPattern = @"OGL -\((\d*)x(\d*)\)x\d* bit";

        private static readonly Regex VideoCardRegex = new Regex(VideoCardPattern);

        #region IConfigurationProvider Members

        public Task WriteConfigurationAsync(IProfile profile, CancellationToken token)
        {
            return Task.Run(() => WriteRegistry(profile, token), token);
        }

        public Task PullConfigurationAsync(IProfile profile, CancellationToken token)
        {
            return Task.Run(() => ReadRegistry(profile, token), token);
        }

        private static void ReadRegistry(IProfile profile, CancellationToken token)
        {
            var registryPath = string.Format(@"{0}_Classes\VirtualStore\Machine\Software\Wow6432Node\Volition\Freespace2", UserSid);

            using (var hkeyCurrentUser = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var fsKey = hkeyCurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (fsKey == null)
                    {
                        return;
                    }

                    var videoSettings = fsKey.GetValue("VideocardFs2open") as string;

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
                            catch (FormatException e)
                            {
                                // Ignore exception
                            }
                        }
                    }

                    var joystickGuid = fsKey.GetValue("CurrentJoystickGUID") as string;

                    if (joystickGuid == null)
                    {
                        var joystickIndex = fsKey.GetValue("CurrentJoystick");

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

                    var textureFilter = fsKey.GetValue("TextureFilter");
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
                }
            }
        }

        #endregion

        private static void WriteRegistry(IProfile profile, CancellationToken token)
        {
            var registryPath = string.Format(@"{0}_Classes\VirtualStore\Machine\Software\Wow6432Node\Volition\Freespace2", UserSid);

            using (var hkeyCurrentUser = RegistryKey.OpenBaseKey(RegistryHive.Users, RegistryView.Default))
            {
                using (var fsKey = hkeyCurrentUser.OpenSubKey(registryPath, RegistryKeyPermissionCheck.ReadWriteSubTree))
                {
                    if (fsKey == null)
                    {
                        return;
                    }

                    var videoString = string.Format("OGL -({0}x{1})x{2} bit", profile.ResolutionWidth, profile.ResolutionHeight, 32);
                    fsKey.SetValue("VideocardFs2open", videoString);

                    if (profile.SelectedJoystickGuid != null)
                    {
                        fsKey.SetValue("CurrentJoystickGUID", profile.SelectedJoystickGuid);

                        var joystickIndex = SDLJoystick.GetJoysticks().IndexOf(joy => joy.GUID == profile.SelectedJoystickGuid);

                        if (joystickIndex >= 0)
                        {
                            fsKey.SetValue("CurrentJoystick", joystickIndex);
                        }
                        else
                        {
                            fsKey.DeleteValue("CurrentJoystick");
                        }
                    }
                    else
                    {
                        fsKey.DeleteValue("CurrentJoystickGUID");
                        fsKey.DeleteValue("CurrentJoystick");
                    }

                    fsKey.SetValue("TextureFilter", (uint) profile.TextureFiltering, RegistryValueKind.DWord);
                }
            }
        }
    }
}
