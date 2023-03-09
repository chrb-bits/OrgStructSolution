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
    /// Interaction logic for NewPositionDialog.xaml
    /// </summary>
    public partial class NewPositionDialog : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public PositionModel ParentPosition { private set;  get; }

        public PositionModel Position { private set;  get; }

        public bool Saved { private set; get; }

        public NewPositionDialog(PositionModel parentPosition)
        {
            ParentPosition = parentPosition;
            Construct();            
        }

        public NewPositionDialog()
        {
            Construct();
        }

        private void Construct()
        {
            InitializeComponent();

            Position = new PositionModel();
            Saved = false;

            if (Program.ConnectionMgr.State == Session.ConnectionManagerState.Connected)
            {
                cmbPosition_Person.ItemsSource = Program.ConnectionMgr.Organization.People.OrderBy(p => p.Name);
                lsvPosition_Roles.ItemsSource = Position.Roles;

                //  todo: make label databound
                if (ParentPosition == null)
                {
                    lblParentPosition.Content = Program.ConnectionMgr.Organization.Name;
                    Program.ConnectionMgr.Organization.PropertyChanged += Organization_PropertyChanged;
                }
                else
                {
                    lblParentPosition.Content = ParentPosition.ToString();
                    ParentPosition.PropertyChanged += ParentPosition_PropertyChanged;
                }
            }
            else
            {
                // no connection
                Close();
            }
        }

        private void ParentPosition_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lblParentPosition.Content = ParentPosition.ToString();
        }

        private void Organization_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            lblParentPosition.Content = Program.ConnectionMgr.Organization.Name;
        }

        private void cmbPosition_Person_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cmbPosition_Person.SelectedItem != null)
            {
                Position.Person = (PersonModel)cmbPosition_Person.SelectedItem;
            }
            else
            {
                Position.Person = null;
            }
        }

        private void btnPosition_PersonNew_Click(object sender, RoutedEventArgs e)
        {
            PersonDialog dlg = new PersonDialog();
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                Program.ConnectionMgr.FetchUpdates();
                Position.Person = dlg.Person;
                cmbPosition_Person.ItemsSource = Program.ConnectionMgr.Organization.People;
                cmbPosition_Person.SelectedItem = Position.Person;
            }
        }

        private void btnPosition_PersonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (cmbPosition_Person.SelectedItem != null)
            {
                PersonModel selPerson = (PersonModel)cmbPosition_Person.SelectedItem;

                Guid selPersonLockId = Guid.Empty;
                try
                {
                    selPersonLockId = Program.ConnectionMgr.LockAcquire(selPerson.ObjectID);

                    PersonDialog dlg = new PersonDialog((PersonModel)cmbPosition_Person.SelectedItem);
                    dlg.PersonLockId = selPersonLockId;
                    dlg.ShowDialog();

                    Program.ConnectionMgr.LockRelease(selPersonLockId);

                    if (dlg.Saved)
                    {
                        Position.Person = dlg.Person;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnPosition_RoleNew_Click(object sender, RoutedEventArgs e)
        {
            // role dialog
            RoleDialog dlg = new RoleDialog();
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                // force client update (added new role to organization)
                Program.ConnectionMgr.FetchUpdates();

                // add new role to position
                Position.Roles.Add(dlg.Role);
            }
        }

        private void btnPosition_RoleEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lsvPosition_Roles.SelectedItem != null)
            {
                RoleModel selRole = (RoleModel)lsvPosition_Roles.SelectedItem;

                Guid selRoleLockId = Guid.Empty;
                try
                {
                    selRoleLockId = Program.ConnectionMgr.LockAcquire(selRole.ObjectID);

                    // role dialog
                    RoleDialog dlg = new RoleDialog(selRole);
                    dlg.RoleLockId = selRoleLockId;
                    dlg.ShowDialog();

                    Program.ConnectionMgr.LockRelease(selRoleLockId);

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

        private void btnPosition_RoleAdd_Click(object sender, RoutedEventArgs e)
        {
            // role select dialog
            SelectRoleDialog dlg = new SelectRoleDialog(Position.Roles);
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                // add selected role
                Position.Roles.Add(dlg.Role);

                // fetch updates
                Program.ConnectionMgr.FetchUpdates();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {            
            Saved = true;
            // no lock required on writing new position, but may need to supply ParentPosition                        
            Close();            
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ParentPosition == null)
            {
                Program.ConnectionMgr.Organization.PropertyChanged -= Organization_PropertyChanged;
            }
            else
            {
                ParentPosition.PropertyChanged -= ParentPosition_PropertyChanged;
            }
        }

        private void btnPosition_RoleRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lsvPosition_Roles.SelectedItem != null)
            {
                RoleModel selRole = (RoleModel)lsvPosition_Roles.SelectedItem;
                Position.Roles.Remove(selRole);
            }
        }
    }
}