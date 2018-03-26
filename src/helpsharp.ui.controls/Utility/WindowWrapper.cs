using System;

namespace csharp.ui.controls.Utility
{
    public class WindowWrapper : System.Windows.Forms.IWin32Window
    {
        #region Private Fields

        private IntPtr _hwnd;

        #endregion Private Fields

        #region Public Constructors

        public WindowWrapper(IntPtr handle)
        {
            _hwnd = handle;
        }

        #endregion Public Constructors

        #region Public Properties

        public IntPtr Handle
        {
            get { return _hwnd; }
        }

        #endregion Public Properties
    }
}