using System;
using System.Windows.Forms;

namespace csharp.ui.controls.Utility
{
    public class WindowWrapper : IWin32Window
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

        IntPtr IWin32Window.Handle => throw new NotImplementedException();

        #endregion Public Properties
    }
}