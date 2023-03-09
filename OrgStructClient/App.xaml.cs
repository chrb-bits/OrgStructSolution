using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using OrgStructClient.Session;

namespace OrgStructClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            ConfigurationRepository.ServerURL = OrgStructClient.Properties.Settings.Default.ServerURL;
            if (ConfigurationRepository.ServerURL == string.Empty) { ConfigurationRepository.ServerURL = "http://localhost:9000/"; }

            SessionMgr = new SessionManager();
            ConnectionMgr = new ConnectionManager();
            
            // set session ID
            ConnectionMgr.SessionID = SessionMgr.SessionID;
        }

        public SessionManager SessionMgr { get; }

        public ConnectionManager ConnectionMgr { get; }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            ConnectionMgr.Disconnect();
        }
    }
}