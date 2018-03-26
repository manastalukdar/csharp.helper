using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Threading;

namespace helpsharp.Utility
{
    public class Miscellaneous
    {
        #region Private Fields

        private const int ShowWindowMinimize = 6;

        private const int ShowWindowRestore = 9;

        private const int ShowWindowShow = 5;

        #endregion Private Fields

        #region Public Enums

        public enum FormatMessageFlags
        {
            FormatMessageAllocateBuffer = 0x00000100,
            FormatMessageIgnoreInserts = 0x00000200,
            FormatMessageFromSystem = 0x00001000,
            FormatMessageArgumentArray = 0x00002000,
            FormatMessageFromHmodule = 0x00000800,
            FormatMessageFromString = 0x00000400
        }

        public enum ProcessState
        {
            Started,
            Stopped
        }

        #endregion Public Enums

        #region Private Enums

        private enum ShowWindowStatus
        {
            SwMinimize = 6,
            SwRestore = 9,
            SwShow = 5
        }

        #endregion Private Enums

        #region Public Methods

        public void ActivateApplication(string briefAppName)
        {
            var procList = Process.GetProcessesByName(briefAppName);

            if (procList.Length > 0)
            {
                ShowWindow(procList[0].MainWindowHandle, ShowWindowRestore);
                SetForegroundWindow(procList[0].MainWindowHandle);
            }
        }

        public void ActivateProcess(Process proc)
        {
            ShowWindow(proc.MainWindowHandle, ShowWindowRestore);
            SetForegroundWindow(proc.MainWindowHandle);
        }

        public string[] FormatDomainAndUsername(string userName)
        {
            var domainAndUsername = new string[2];
            var domain = Environment.MachineName;
            if (userName.IndexOf("\\", StringComparison.Ordinal) != -1)
            {
                var index = userName.LastIndexOf("\\", StringComparison.Ordinal);
                if (domain != ".")
                {
                    domain = userName.Substring(0, index);
                }
                else
                {
                    domain = ".";
                }

                userName = userName.Replace(domain + "\\", string.Empty);
            }

            domainAndUsername[0] = domain;
            domainAndUsername[1] = userName;
            return domainAndUsername;
        }

        public string FormatMessage(uint errorCode)
        {
            var msgBuf = IntPtr.Zero;

            var ret = FormatMessage(
                FormatMessageFlags.FormatMessageAllocateBuffer | FormatMessageFlags.FormatMessageFromSystem | FormatMessageFlags.FormatMessageIgnoreInserts,
                IntPtr.Zero,
                errorCode,
                0, // Default language
                ref msgBuf,
                0,
                IntPtr.Zero);

            if (ret == 0)
            {
                // Handle the error.
                throw new ApplicationException("FormatMessage failed.  Win32 Error Code: " +
                                                   Marshal.GetLastWin32Error() + "|| Message: " +
                     new Win32Exception(Marshal.GetLastWin32Error()).Message);
            }
            else
            {
                var message = Marshal.PtrToStringAnsi(msgBuf);
                return message;
            }
        }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <param name="stringToParse">The string to parse.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="partToReturn">The part to return.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">stringToParse or separator</exception>
        public string GetField(string stringToParse, string separator, int partToReturn)
        {
            if (stringToParse == null)
            {
                throw new ArgumentNullException("stringToParse");
            }

            if (separator == null)
            {
                throw new ArgumentNullException("separator");
            }

            int count1 = 0, count2 = 0, count3 = 0;
            var array = new string[1000];
            IEnumerator stringEnum = stringToParse.GetEnumerator();
            while (stringEnum.MoveNext())
            {
                count1++;
                if (stringEnum.Current.ToString().Equals(separator))
                {
                    array[count3] = stringToParse.Substring(count2, count1 - count2 - 1);
                    count2 = count1;
                    count3++;
                }
            }

            if (count3 != 0)
            {
                array[count3] = stringToParse.Substring(count2, count1 - count2); // iCount2+1
            }
            else if (count3 == 0)
            {
                array[count3] = stringToParse;
                return array[count3];
            }

            return array[partToReturn - 1];
        }

        public string GetFullExceptionMessage(Exception ex)
        {
            var message = ex.Message;
            while (ex.InnerException != null)
            {
                message += "||InnerException: " + ex.InnerException.Message;
                ex = ex.InnerException;
            }

            return message;
        }

        /// <summary>
        /// Gets the X element position in array.
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="element">The xe var.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentNullException">array or xeVar</exception>
        public int GetXElementPositionInArray(XElement[] array, XElement element)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (element == null)
            {
                throw new ArgumentNullException("xeVar");
            }

            var count1 = 0;
            foreach (var var in array)
            {
                count1++;
                if (var == element)
                {
                    return count1 - 1;
                }
            }

            return -1;
        }

        /// <summary>
        /// Determines whether [is int in array] [the specified array].
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="num">The num.</param>
        /// <returns><c>true</c> if [is int in array] [the specified array]; otherwise, <c>false</c>.</returns>
        /// <exception cref="System.ArgumentNullException">array</exception>
        public bool IsIntInArray(int[] array, int num)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            var count1 = array.Count(count2 => count2 == num);
            return count1 != 0;
        }

        /// <summary>
        /// Determines whether [is X element in array] [the specified array].
        /// </summary>
        /// <param name="array">The array.</param>
        /// <param name="element">The xe var.</param>
        /// <returns>
        /// <c>true</c> if [is X element in array] [the specified array]; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="System.ArgumentNullException">array or xeVar</exception>
        public bool IsXElementInArray(XElement[] array, XElement element)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            if (element == null)
            {
                throw new ArgumentNullException("xeVar");
            }

            var count1 = array.Count(var => var == element);
            return count1 != 0;
        }

        /// <summary>
        /// Matches the string.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        public bool MatchString(string str, string pattern)
        {
            if (string.IsNullOrEmpty(str) && string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            if (string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(str) && string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            ////sPattern = @sPattern.Replace("\\", "\\\\");
            ////sString = sString.Replace("\\", "\\\\");
            var success = str != null && str.Equals(pattern, StringComparison.OrdinalIgnoreCase);
            ////int iTest = String.Compare(sString, sPattern, true);
            ////Match matchString = Regex.Match(sString, sPattern, RegexOptions.IgnoreCase);
            ////Regex regex = new Regex(@sPattern, RegexOptions.IgnoreCase);
            ////Match matchString = regex.Match(sString);
            return success;
        }

        /// <summary>
        /// Matches the string reg perm env var.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="pattern">The pattern.</param>
        /// <returns></returns>
        public bool MatchStringRegPermEnvVar(string str, string pattern)
        {
            if (string.IsNullOrEmpty(str) && string.IsNullOrEmpty(pattern))
            {
                return true;
            }

            if (string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(str) && string.IsNullOrEmpty(pattern))
            {
                return false;
            }

            if (pattern != null)
            {
                pattern = pattern.Replace("\\", "\\\\");
            }

            if (str != null)
            {
                if (pattern != null)
                {
                    var matchString = Regex.Match(str, pattern, RegexOptions.IgnoreCase);
                    ////Regex regex = new Regex(@sPattern, RegexOptions.IgnoreCase);
                    ////Match matchString = regex.Match(sString);
                    return matchString.Success;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes null character.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns></returns>
        public byte[] NullRemover(byte[] dataStream)
        {
            int i;
            var size = dataStream.Length;
            var temp = new byte[size];
            for (i = 0; i < size - 1; i++)
            {
                if (dataStream[i] == 0x00)
                {
                    break;
                }

                temp[i] = dataStream[i];
            }

            var nullLessDataStream = new byte[i];
            for (i = 0; i < nullLessDataStream.Length; i++)
            {
                nullLessDataStream[i] = temp[i];
            }

            return nullLessDataStream;
        }

        /// <summary>
        /// Removes null character.
        /// </summary>
        /// <param name="dataStream">The data stream.</param>
        /// <returns></returns>
        public byte[] NullRemover2(byte[] dataStream)
        {
            int i;
            var count = -1;
            var size = dataStream.Length;
            var temp = new byte[size];
            for (i = 0; i < size - 1; i++)
            {
                if (dataStream[i] != 0x00)
                {
                    count++;
                    temp[count] = dataStream[i];
                }
            }

            var nullLessDataStream = new byte[i];
            for (i = 0; i < nullLessDataStream.Length; i++)
            {
                nullLessDataStream[i] = temp[i];
            }

            return nullLessDataStream;
        }


        /// <summary>
        /// Waits for process to stop.
        /// </summary>
        /// <param name="timeoutInSeconds">The timeout in seconds.</param>
        /// <param name="state">The state.</param>
        /// <param name="processName">Name of the process.</param>
        public void WaitForProcessToStop(int timeoutInSeconds, ProcessState state, string processName)
        {
            var maxDuration = TimeSpan.FromSeconds(timeoutInSeconds);
            var sw = Stopwatch.StartNew();
            if (state == ProcessState.Started)
            {
                while (Process.GetProcessesByName(processName).Length == 0 && sw.Elapsed < maxDuration)
                {
                    Thread.Sleep(2000);
                }
            }
            else if (state == ProcessState.Stopped)
            {
                while (Process.GetProcessesByName(processName).Length != 0 && sw.Elapsed < maxDuration)
                {
                    Thread.Sleep(2000);
                }
            }
        }

        /// <summary>
        /// Determines whether [is process running] [the specified process name].
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception"></exception>
        public bool IsProcessRunning(string processName)
        {

            var processes = Process.GetProcessesByName(processName);
            var numOfProcesses = processes.Length;

            if (numOfProcesses == 0)
            {
                return false;
            }
            else if (numOfProcesses > 1)
            {
                throw new Exception(string.Format("More than one {0} process detected.", processName));
            }

            // Not 0 or > 1 so is 1.
            return true;
        }

        #endregion Public Methods

        #region Private Methods

        // the version, the sample is built upon:
        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern uint FormatMessage(FormatMessageFlags flags,
            IntPtr source,
            uint messageId,
            uint languageId,
            ref IntPtr buffer,
            uint size,
            IntPtr arguments);

        [DllImport("User32")]
        private static extern int SetForegroundWindow(IntPtr wnd);

        // Activate or minimize a window
        [DllImport("User32.DLL")]
        private static extern bool ShowWindow(IntPtr wnd, int cmdShow);

        #endregion Private Methods
    }
}