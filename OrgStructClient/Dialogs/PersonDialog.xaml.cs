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
using System.Windows.Threading;
using OrgStructModels.Metadata;
using OrgStructModels.Persistables;

namespace OrgStructClient
{
    /// <summary>
    /// Interaction logic for PersonDialog.xaml
    /// </summary>
    public partial class PersonDialog : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public PersonModel Person { set; get; }

        public Guid PersonLockId { set; get; }

        public bool Saved { private set; get; }

        private bool AllowSave { get { return (txtFirstName.Text.Trim() != string.Empty) || (txtName.Text.Trim() != string.Empty); } }

        private DispatcherTimer lockRefreshTimer;

        public PersonDialog()
        {
            InitializeComponent();

            lockRefreshTimer = new DispatcherTimer();

            // start with new person
            Person = new PersonModel();
            Saved = false;
        }

        public PersonDialog(PersonModel person)
        {
            InitializeComponent();

            lockRefreshTimer = new DispatcherTimer();
            lockRefreshTimer.Interval = new TimeSpan(0, 0, 30);
            lockRefreshTimer.Tick += LockRefreshTimer_Tick;

            // load supplied person
            Person = person;
            Saved = false;
            UpdateDisplay();

            lockRefreshTimer.Start();
        }

        private void LockRefreshTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                PersonLockId = Program.ConnectionMgr.LockAcquire(Person.ObjectID);
            }
            catch (Exception ex)
            {
                lockRefreshTimer.Stop();
                MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        ~PersonDialog()
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
            txtFirstName.Text = Person.FirstName;
            txtName.Text = Person.Name;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            lockRefreshTimer.Stop();

            // update person
            Person.FirstName = txtFirstName.Text.Trim();
            Person.Name = txtName.Text.Trim();

            // write person to server
            Program.ConnectionMgr.PersonWrite(PersonLockId, Person);

            Saved = true;
            
            // done
            Close();
        }

        private void txtFirstName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSave.IsEnabled = AllowSave;
        }

        private void txtName_TextChanged(object sender, TextChangedEventArgs e)
        {
            btnSave.IsEnabled = AllowSave;
        }
    }
}
