using OrgStructModels.Persistables;
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
    /// Interaction logic for ManageRoles.xaml
    /// </summary>
    public partial class ManageRolesDialog : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public PositionModel Position { set; get; }

        public ManageRolesDialog()
        {
            InitializeComponent();

            if (Program.ConnectionMgr.State == Session.ConnectionManagerState.Connected)
            {
                RefreshRolesList(this, new EventArgs());
            }

        }

        ~ManageRolesDialog()
        {
            if (Program?.ConnectionMgr != null)
            {
                Program.ConnectionMgr.Refreshed -= RefreshRolesList;
                Program.ConnectionMgr.Organization.Roles.ListChanged -= RefreshRolesList;
            }
        }

        private void RefreshRolesList(object sender, EventArgs e)
        {
            if (Program != null)
            {
                // unhook list changed event
                Program.ConnectionMgr.Refreshed -= RefreshRolesList;
                Program.ConnectionMgr.Organization.Roles.ListChanged -= RefreshRolesList;

                // rebind list
                Task.Delay(0).ContinueWith(t => Application.Current.Dispatcher.Invoke(() => { lsvOrgRoles.ItemsSource = null; lsvOrgRoles.ItemsSource = Program.ConnectionMgr.Organization.Roles.OrderBy(r => r.Name); }));

                // rehook events
                Program.ConnectionMgr.Refreshed += RefreshRolesList;
                Program.ConnectionMgr.Organization.Roles.ListChanged += RefreshRolesList;
            }
        }

        private void btnRoleNew_Click(object sender, RoutedEventArgs e)
        {
            // role dialog
            RoleDialog dlg = new RoleDialog();
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                // force client update (added new role to organization)
                Program.ConnectionMgr.FetchUpdates();

                lsvOrgRoles.ItemsSource = Program.ConnectionMgr.Organization.Roles.OrderBy(r => r.Name);
            }
        }

        private void btnRoleEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lsvOrgRoles.SelectedItem != null)
            {
                RoleModel selRole = (RoleModel)lsvOrgRoles.SelectedItem;

                try
                {
                    Guid roleLockId = Program.ConnectionMgr.LockAcquire(selRole.ObjectID);

                    // role dialog
                    RoleDialog dlg = new RoleDialog((RoleModel)lsvOrgRoles.SelectedItem);
                    dlg.RoleLockId = roleLockId;
                    dlg.ShowDialog();

                    Program.ConnectionMgr.LockRelease(roleLockId);

                    if (dlg.Saved)
                    {
                        // force client update (added new role to organization)
                        Program.ConnectionMgr.FetchUpdates();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnRoleDelete_Click(object sender, RoutedEventArgs e)
        {
            if (lsvOrgRoles.SelectedItem != null)
            {
                RoleModel selRole = (RoleModel)lsvOrgRoles.SelectedItem;

                try
                {
                    Guid roleLockId = Program.ConnectionMgr.LockAcquire(selRole.ObjectID);

                    Program.ConnectionMgr.RoleDelete(roleLockId, (RoleModel)lsvOrgRoles.SelectedItem);
                    RoleModel roleToRemove = Program.ConnectionMgr.Organization.Roles.Where(p => p.ObjectID == selRole.ObjectID).FirstOrDefault();
                    if (roleToRemove != null) { Program.ConnectionMgr.Organization.Roles.Remove(roleToRemove); }

                    Program.ConnectionMgr.LockRelease(roleLockId);

                    Program.ConnectionMgr.FetchUpdates();

                    lsvOrgRoles.ItemsSource = Program.ConnectionMgr.Organization.Roles.OrderBy(p => p.Name);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
