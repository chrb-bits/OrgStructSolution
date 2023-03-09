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
    /// Interaction logic for ManagePeople.xaml
    /// </summary>
    public partial class ManagePeopleDialog : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public ManagePeopleDialog()
        {
            InitializeComponent();

            if (Program.ConnectionMgr.State == Session.ConnectionManagerState.Connected)
            {
                RefreshPeopleList(this, new EventArgs());
            }
        }

        ~ManagePeopleDialog()
        {
            if (Program?.ConnectionMgr != null)
            {
                Program.ConnectionMgr.Refreshed -= RefreshPeopleList;
                Program.ConnectionMgr.Organization.People.ListChanged -= RefreshPeopleList;
            }
        }

        private void RefreshPeopleList(object sender, EventArgs e)
        {
            // force list refresh
            if (Program != null)
            {
                // unhook list changed events
                Program.ConnectionMgr.Refreshed -= RefreshPeopleList;
                Program.ConnectionMgr.Organization.People.ListChanged -= RefreshPeopleList;

                // rebind list
                Task.Delay(0).ContinueWith(t => Application.Current.Dispatcher.Invoke(() => { lsvOrgPeople.ItemsSource = null; lsvOrgPeople.ItemsSource = Program.ConnectionMgr.Organization.People.OrderBy(p => p.Name); }));

                // rehook events
                Program.ConnectionMgr.Refreshed += RefreshPeopleList;
                Program.ConnectionMgr.Organization.People.ListChanged += RefreshPeopleList;
            }            
        }

        private void btnPersonDelete_Click(object sender, RoutedEventArgs e)
        {
           if (lsvOrgPeople.SelectedItem != null)
            {
                PersonModel selPerson = (PersonModel)lsvOrgPeople.SelectedItem;

                try
                {
                    Guid personLockId = Program.ConnectionMgr.LockAcquire(selPerson.ObjectID);

                    Program.ConnectionMgr.PersonDelete(personLockId, selPerson);
                    PersonModel personToRemove = Program.ConnectionMgr.Organization.People.Where(p => p.ObjectID == selPerson.ObjectID).FirstOrDefault();
                    if (personToRemove != null) { Program.ConnectionMgr.Organization.People.Remove(personToRemove); }

                    Program.ConnectionMgr.LockRelease(personLockId);

                    Program.ConnectionMgr.FetchUpdates();

                    lsvOrgPeople.ItemsSource = Program.ConnectionMgr.Organization.People.OrderBy(p => p.Name);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }            
        }

        private void btnPersonNew_Click(object sender, RoutedEventArgs e)
        {
            // role dialog
            PersonDialog dlg = new PersonDialog();
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                // force client update (added new role to organization)
                Program.ConnectionMgr.FetchUpdates();

                //Program.ConnectionMgr.Organization.People.Add(dlg.Person);
                lsvOrgPeople.ItemsSource = Program.ConnectionMgr.Organization.People.OrderBy(p => p.Name);
            }
        }

        private void btnPersonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (lsvOrgPeople.SelectedItem != null)
            {
                PersonModel selPerson = (PersonModel)lsvOrgPeople.SelectedItem;

                try
                {
                    Guid personLockId = Program.ConnectionMgr.LockAcquire(selPerson.ObjectID);

                    // person dialog
                    PersonDialog dlg = new PersonDialog((PersonModel)lsvOrgPeople.SelectedItem);
                    dlg.PersonLockId = personLockId;
                    dlg.ShowDialog();

                    Program.ConnectionMgr.LockRelease(personLockId);

                    if (dlg.Saved)
                    {
                        // force client update (added new role to organization)
                        Program.ConnectionMgr.FetchUpdates();

                        lsvOrgPeople.ItemsSource = Program.ConnectionMgr.Organization.People.OrderBy(p => p.Name);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
