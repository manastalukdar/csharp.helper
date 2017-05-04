using System.Net;

namespace csharp.helper.Utility
{
    public class WebUtils
    {
        #region Public Methods

        public bool CanResolveDns(string hostNameOrAddress)
        {
            IPHostEntry host;
            host = Dns.GetHostEntry(hostNameOrAddress);
            if (host.AddressList.Length > 0)
            {
                return true;
            }

            return false;
        }

        #endregion Public Methods
    }
}