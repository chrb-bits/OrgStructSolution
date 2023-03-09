using OrgStructModels.Metadata;
using OrgStructModels.Persistables;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for SelectRoleDialog.xaml
    /// </summary>
    public partial class SelectRoleDialog : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public RoleModel Role { set; get; }

        public ObjectLock RoleLock { set; get; }

        public bool Saved { private set; get; }

        private bool AllowSave { get { return cmbRole.SelectedItem != null; } }

        public SelectRoleDialog(BindingList<RoleModel> excludedRolesList)
        {
            InitializeComponent();
            Role = null;
            //cmbRole.ItemsSource = Program.ConnectionMgr.Organization.Roles;

            //IEnumerable<RoleModel> res = (excludedRolesList.Where(x => Program.ConnectionMgr.Organization.Roles.All(y => y.ObjectID != x.ObjectID)));
            IEnumerable<RoleModel> res = Program.ConnectionMgr.Organization.Roles.Where(x => excludedRolesList.All(y => x.ObjectID != y.ObjectID));
            cmbRole.ItemsSource = res.ToList();

            cmbRole.SelectedItem = cmbRole.Items.GetItemAt(0);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {

            // update role
            Role = (RoleModel)cmbRole.SelectedItem;

            Saved = true;

            // done
            Close();
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {

        }
    }
}
