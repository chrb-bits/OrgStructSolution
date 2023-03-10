using System;

namespace OrgStructModels.Metadata
{
    // event handler delegate
    //public delegate void PersistenceLogEventHandler(object sender, LogEventArgs e);

    /// <summary>
    /// Common Log Message Event
    /// </summary>
    public class LogEventArgs : EventArgs
    {
        public LogEventArgs(string logMessage) : base()
        {
            Message = logMessage;
        }

        /// <summary>
        /// The log message.
        /// </summary>
        public string Message { private set; get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
