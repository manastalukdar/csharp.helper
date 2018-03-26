using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security;
using System.ServiceProcess;

namespace helpsharp.Utility
{
    public class Services
    {              
        #region Private Fields

        private const int ErrorInsufficientBuffer = 122;

        private const uint ServiceChangeConfig = 0x00002;
        private const uint ServiceControlManagerAllAccess = 0x000F003F;
        private const uint ServiceNoChange = 0xffffffff;

        private const uint ServiceQueryConfig = 0x00001;

        #endregion Private Fields

        #region Public Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ChangeServiceConfig(IntPtr service, uint serviceType, uint startType,
            uint errorControl, string binaryPathName, string loadOrderGroup, IntPtr tagId, string dependencies,
            string serviceStartName, string password, string displayName);

        public static void ChangeStartMode(ServiceController svc, ServiceStartMode mode)
        {
            var managerHandle = OpenSCManager(null, null, ServiceControlManagerAllAccess);
            if (managerHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Manager Error");
            }

            var serviceHandle = OpenService(
                managerHandle,
                svc.ServiceName,
                ServiceQueryConfig | ServiceChangeConfig);

            if (serviceHandle == IntPtr.Zero)
            {
                throw new ExternalException("Open Service Error");
            }

            var result = ChangeServiceConfig(
                serviceHandle,
                ServiceNoChange,
                (uint)mode,
                ServiceNoChange,
                null,
                null,
                IntPtr.Zero,
                null,
                null,
                null,
                null);

            if (result == false)
            {
                var error = Marshal.GetLastWin32Error();
                var win32Exception = new Win32Exception(error);
                throw new ExternalException("Could not change service start type: "
                    + win32Exception.Message);
            }

            CloseServiceHandle(serviceHandle);
            CloseServiceHandle(managerHandle);
        }

        [DllImport("advapi32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool QueryServiceConfig(IntPtr service, IntPtr intPtrQueryConfig, uint bufSize, out uint bytesNeeded);

        public static void SetWindowsServiceCreds(string serviceName, string username, string password)
        {
            var manager = IntPtr.Zero;
            var service = IntPtr.Zero;
            try
            {
                manager = OpenSCManager(null, null, ServiceControlManagerAllAccess);
                if (manager == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                service = OpenService(manager, serviceName, ServiceQueryConfig | ServiceChangeConfig);
                if (service == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                if (!ChangeServiceConfig(service, ServiceNoChange, ServiceNoChange, ServiceNoChange, null, null, IntPtr.Zero, null, username, password, null))
                {
                    throw new Win32Exception();
                }
            }
            finally
            {
                if (service != IntPtr.Zero)
                {
                    CloseServiceHandle(service);
                }

                if (manager != IntPtr.Zero)
                {
                    CloseServiceHandle(manager);
                }
            }
        }

        public uint GetProcessIdByServiceDisplayName(string serviceDisplayName)
        {
            uint processId = 0;
            var qry = "SELECT PROCESSID FROM WIN32_SERVICE WHERE DISPLAYNAME = '" + serviceDisplayName + "'";
            var searcher = new ManagementObjectSearcher(qry);
            foreach (ManagementObject mngntObj in searcher.Get())
            {
                processId = (uint)mngntObj["PROCESSID"];
            }

            if (processId == 0)
            {
                throw new ApplicationException("Unable to get processId of service");
            }

            return processId;
        }

        public uint GetProcessIdByServiceName(string serviceName)
        {
            uint processId = 0;
            var qry = "SELECT PROCESSID FROM WIN32_SERVICE WHERE NAME = '" + serviceName + "'";
            var searcher = new ManagementObjectSearcher(qry);
            foreach (ManagementObject mngntObj in searcher.Get())
            {
                processId = (uint)mngntObj["PROCESSID"];
            }

            if (processId == 0)
            {
                throw new ApplicationException("Unable to get processId of service");
            }

            return processId;
        }

        public List<ServiceController> GetRunningServices()
        {
            var services = ServiceController.GetServices();

            return services.Where(item => item.Status == ServiceControllerStatus.Running).ToList();
        }

        public string GetServiceLogOnUser(string serviceName)
        {
            var servicelogondetails = string.Empty;
            var query = new SelectQuery(string.Format("select name, startname from Win32_Service where name = '{0}'",
                serviceName));
            using (var mgmtSearcher = new ManagementObjectSearcher(query))
            {
                foreach (ManagementObject service in mgmtSearcher.Get())
                {
                    servicelogondetails = service["startname"].ToString();
                }
            }

            if (servicelogondetails.Length == 0)
            {
                throw new ApplicationException("Unable to get service LogOn username");
            }

            return servicelogondetails;
        }

        public string GetServiceStatus(string serviceName)
        {
            var sc = new ServiceController(serviceName);
            return sc.Status.ToString();
        }

        public bool IsServiceRunning(string serviveName)
        {
            var sc = new ServiceController(serviveName);
            return sc.Status.Equals(ServiceControllerStatus.Running);
        }

        public bool IsServiceStopped(string serviceName)
        {
            var sc = new ServiceController(serviceName);
            return sc.Status.Equals(ServiceControllerStatus.Stopped);
        }

        public bool ServiceExists(string serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentNullException("Service name");

            var services = ServiceController.GetServices();

            // try to find service name
            return services.Any(service => service.ServiceName.Equals(serviceName, StringComparison.CurrentCultureIgnoreCase));
        }

        public void SetServiceLogOnUser(string serviceName, string userName, SecureString password)
        {
            var objPath = string.Format("Win32_Service.Name='{0}'", serviceName);
            using (var service = new ManagementObject(new ManagementPath(objPath)))
            {
                var wmiParams = new object[11];
                wmiParams[6] = userName;
                wmiParams[7] = password.ToString(); // [Manas]: possible security issue

                var invokeResult = service.InvokeMethod("Change", wmiParams);

                // handle invokeResult - no error up to this point
            }
        }

        public void StartService(string serviceName, int timeoutSeconds = 30)
        {
            if (ServiceExists(serviceName))
            {
                if (!IsServiceRunning(serviceName))
                {
                    StartServiceBase(serviceName, timeoutSeconds);
                }
            }
        }

        public void StopService(string serviceName, int timeoutSeconds = 30)
        {
            if (ServiceExists(serviceName))
            {
                if (IsServiceRunning(serviceName))
                {
                    StopServiceBase(serviceName, timeoutSeconds);
                }
            }
        }

        #endregion Public Methods

        #region Internal Methods

        [DllImport("advapi32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool CloseServiceHandle(IntPtr handle);

        [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", ExactSpelling = true, CharSet = CharSet.Unicode, SetLastError = true)]
        internal static extern IntPtr OpenSCManager(
             string machineName,
             string databaseName,
             uint access);

        #endregion Internal Methods

        #region Private Methods

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern IntPtr OpenService(IntPtr manager, string serviceName, uint desiredAccess);



        private void StartServiceBase(string serviceName, int timeoutSeconds = 30)
        {
            var service = new ServiceController(serviceName);
            try
            {
                var timeout = TimeSpan.FromMilliseconds(timeoutSeconds * 1000);

                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to start service. Exception: " + ex.Message);
            }
        }

        private void StopServiceBase(string serviceName, int timeoutSeconds = 30)
        {
            var service = new ServiceController(serviceName);
            try
            {
                var timeout = TimeSpan.FromMilliseconds(timeoutSeconds * 1000);

                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to stop service. Exception: " + ex.Message);
            }
        }

        #endregion Private Methods

        #region Public Classes

        [StructLayout(LayoutKind.Sequential)]
        public class QUERY_SERVICE_CONFIG
        {
            [MarshalAs(UnmanagedType.U4)]
            private uint serviceType;

            [MarshalAs(UnmanagedType.U4)]
            private uint startType;

            [MarshalAs(UnmanagedType.U4)]
            private uint errorControl;

            [MarshalAs(UnmanagedType.LPWStr)]
            private string binaryPathName;

            [MarshalAs(UnmanagedType.LPWStr)]
            private string loadOrderGroup;

            [MarshalAs(UnmanagedType.U4)]
            private uint tagID;

            [MarshalAs(UnmanagedType.LPWStr)]
            private string dependencies;

            [MarshalAs(UnmanagedType.LPWStr)]
            private string serviceStartName;

            [MarshalAs(UnmanagedType.LPWStr)]
            private string displayName;
        }

        #endregion Public Classes
    }
}