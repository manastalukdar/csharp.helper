using System;
using System.IO;

namespace helpsharp.Utility
{
    public class LocalLogging
    {
        #region Private Fields

        private readonly StreamWriter _file;
        private readonly string _logFile;
        private string _message;
        private string _sessionLogMessages;

        #endregion Private Fields

        #region Public Constructors

        // [Manas]>If file locked throw error instead of crashing
        public LocalLogging(string fileNameAndPath)
        {
            _logFile = fileNameAndPath;

            if (!File.Exists(_logFile))
            {
                var fs = File.Create(_logFile);
                fs.Close();
            }

            _file = new StreamWriter(_logFile, true);
            var date1 = DateTime.Now;
            var utcDateTime = DateTime.UtcNow;
            var datetime = string.Format("{0} | UTC: {1}", date1.ToString("F"), utcDateTime.ToString("F"));
            ////var callStack = new System.Diagnostics.StackTrace(true);
            WriteMessageToLog("---------------------------------");
            WriteMessageToLog(datetime);
            WriteMessageToLog("---------------------------------");
        }

        #endregion Public Constructors

        #region Private Destructors

        ~LocalLogging()
        {
            CloseStreamWriter();
        }

        #endregion Private Destructors

        #region Public Properties

        public string LogFile
        {
            get { return _logFile; }
        }

        public string Message
        {
            get
            {
                return _message;
            }

            set
            {
                _message = value;
            }
        }

        public string SessionLogMessages
        {
            get { return _sessionLogMessages; }
        }

        #endregion Public Properties

        #region Public Methods

        public void CloseStreamWriter()
        {
            try
            {
                _file.Close();
            }
            catch
            {
                // file has been closed
            }
        }

        public void DisplayErrorMessage(string message)
        {
            _file.WriteLine("Error: " + message);
            _file.Flush();
        }

        public void WriteMessageToLog(string msg)
        {
            _message = msg;
            _file.WriteLine(msg);
            _file.Flush();
            if (string.IsNullOrEmpty(_sessionLogMessages))
            {
                _sessionLogMessages = msg;
            }
            else
            {
                _sessionLogMessages = _sessionLogMessages + Environment.NewLine + msg;
            }
        }

        #endregion Public Methods
    }
}