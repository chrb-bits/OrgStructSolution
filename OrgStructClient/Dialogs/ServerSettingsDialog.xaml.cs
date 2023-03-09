using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace OrgStructClient
{
    /// <summary>
    /// Interaction logic for ServerSettiings.xaml
    /// </summary>
    public partial class ServerSettings : Window
    {

        public bool Saved { private set;  get; }
        
        public ServerSettings()
        {
            InitializeComponent();

            Saved = false;
            txtServiceURL.Text = ConfigurationRepository.ServerURL;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (txtServiceURL.Text.Trim() != string.Empty)
            {
                if (!txtServiceURL.Text.EndsWith("/"))
                {
                    txtServiceURL.Text += "/";
                }                
            }
            else
            {
                txtServiceURL.Text = string.Empty;
            }

            // update config repo
            ConfigurationRepository.ServerURL = txtServiceURL.Text;
            
            Saved = true;

            Close();
        }
    }
}