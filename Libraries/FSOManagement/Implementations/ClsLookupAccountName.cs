using System;
using System.Runtime.InteropServices;
using System.Security.Principal;

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
        }

        #endregion

        #region Nested type: TOKEN_USER

        public struct TOKEN_USER
        {
            public SID_AND_ATTRIBUTES User;
        }

        #endregion
    }
}