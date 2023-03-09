using OrgStructModels.Metadata;
using OrgStructModels.Persistables;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OrgStructClient
{
    /// <summary>
    /// Interaction logic for RoleDialog.xaml
    /// </summary>
    public partial class RoleDialog : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public RoleModel Role { set; get; }

        public Guid RoleLockId { set; get; }

        public bool Saved { private set; get; }

        private bool AllowSave { get { return txtName.Text.Trim() != string.Empty; } }

        private DispatcherTimer lockRefreshTimer;

        public RoleDialog()
        {
            InitializeComponent();

            lockRefreshTimer = new DispatcherTimer();

            // start with new person
            Role = new RoleModel();

        }

        private void LockRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                RoleLockId = Program.ConnectionMgr.LockAcquire(Role.ObjectID);
            }
            catch (Exception ex)
            {
                lockRefreshTimer.Stop();
                MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        public RoleDialog(RoleModel role)
        {
            InitializeComponent();

            lockRefreshTimer = new DispatcherTimer();
            lockRefreshTimer.Interval = new TimeSpan(0, 0, 30);
            lockRefreshTimer.Tick += LockRefreshTimer_Tick;
            
            // load supplied person
            Role = role;
            UpdateDisplay();

            lockRefreshTimer.Start();
        }

        ~RoleDialog()
        {
            if (lockRefreshTimer != null)
            {
                lockRefreshTimer.Tick -= LockRefreshTimer_Tick;
                lockRefreshTimer.Stop();
                lockRefreshTimer = null;
            }
        }

        private void UpdateDisplay()
        {
            txtName.Text = Role.Name;
            btnSave.IsEnabled = AllowSave;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lockRefreshTimer.Stop();

            // update role
            Role.Name = txtName.Text.Trim();

            // write person to server
            Program.ConnectionMgr.RoleWrite(RoleLockId, Role);

            Saved = true;

            // done
            Close();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            Role.Name = txtName.Text;
        }
    }
}
