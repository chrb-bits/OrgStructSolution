using System;

namespace OrgStructModels.Metadata
{
    // event handler delegate
    public delegate void PersistenceLogEventHandler(object sender, LogEventArgs e);

    public class LogEventArgs : EventArgs
    {
        public LogEventArgs() : base()
        {

        }
        public LogEventArgs(string logMessage) : base()
        {
            Message = logMessage;
        }

        public string Message { private set; get; }

        public override string ToString()
        {
            return Message;
        }
    }
}
