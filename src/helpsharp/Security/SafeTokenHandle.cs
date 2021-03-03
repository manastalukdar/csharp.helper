using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.Win32.SafeHandles;

namespace helpsharp.Security
{
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        #region Private Constructors

        private SafeTokenHandle()
            : base(true)
        {
        }

        #endregion Private Constructors

        #region Protected Methods

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }

        #endregion Protected Methods

        #region Private Methods

        [DllImport("kernel32.dll")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        #endregion Private Methods
    }
}
