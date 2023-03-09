using Newtonsoft.Json;
using OrgStructModels.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using OrgStructModels.Persistables;
using System.Windows.Controls;
using OrgStructClient.Session;
using System.ComponentModel;
using OrgStructModels.Metadata;
using OrgStructModels.Persistence;
using System.Windows.Media;
using System.Windows.Threading;

namespace OrgStructClient
{
    /// <summary>
    /// Interaction logic for Main.xaml
    /// The whole client code is one hot mess. So much to do, so little time !!!
    /// </summary>
    public partial class Main : Window
    {
        private App Program { get { return ((App)Application.Current); } }

        public OrganizationModel Organization { get { return Program.ConnectionMgr.Organization; } }

        public RootPosition OrgStructureWrapped { get { return Program.ConnectionMgr.OrgStructureWrapped; } }

        public BindingList<PersonModel> OrgPeople { get { return Organization.People; } }

        public PositionModel CurrentEditorPosition { private set; get; }

        public Guid CurrentEditorPositionLockId { private set; get; }

        public bool IsEditing { private set; get; }

        public bool IsRebinding { private set; get; }

        private DispatcherTimer lockUpdateTimer = new DispatcherTimer();

        public Main()
        {
            InitializeComponent();

            InitializeGUI();

            Program.ConnectionMgr.ConnectionManagerEvent += ConnectionMgr_ConnectionManagerEvent;
            Program.ConnectionMgr.Refreshed += ConnectionMgr_Refreshed;
            Program.ConnectionMgr.Updated += ConnectionMgr_Updated;
            IsVisibleChanged += Main_IsVisibleChanged;
            lockUpdateTimer.Tick += LockUpdateTimer_Tick;
        }

        ~Main()
        {
            lockUpdateTimer.Tick -= LockUpdateTimer_Tick;
        }

        private void LockUpdateTimer_Tick(object sender, EventArgs e)
        {
            if (Program.ConnectionMgr.State == ConnectionManagerState.Connected)
            {
                if (IsEditing && (CurrentEditorPositionLockId != Guid.Empty))
                {
                    System.Diagnostics.Debug.WriteLine("EDITOR LOCK REFRESH");
                    // refresh editor lock
                    Program.ConnectionMgr.LockAcquire(CurrentEditorPosition.ObjectID);
                }
            }
        }

        private void ConnectionMgr_Updated(object sender, EventArgs e)
        {
            if (CurrentEditorPosition != null)
            {
                PositionEditorRefresh();
            }            
        }


        private void ConnectionMgr_Refreshed(object sender, EventArgs e)
        {
            // will not process itemchanged event on treeview while IsRebinding is true
            IsRebinding = true;

            // try to obtain currently selected Persistable's objectid
            Guid selObjectID = Guid.Empty;

            if (trvPositions.SelectedItem != null)
            {
                // only do this for Persistables
                if (trvPositions.SelectedItem is IPersistable)
                {
                    // store object id
                    PositionModel selPos = (PositionModel)trvPositions.SelectedItem;
                    selObjectID = selPos.ObjectID;
                }
            }

            // rebind treeview to force updates
            trvPositions.ItemsSource = null;
            trvPositions.ItemsSource = Program.ConnectionMgr.OrgStructureWrapped;

            // restore normal processing for treeview itemchanged event
            IsRebinding = false;

            // try restore item selection by stored object id
            PositionModel selPosition = Program.ConnectionMgr.Organization.Positions.Where(p => p.ObjectID == selObjectID).FirstOrDefault();

            if (selPosition != null)
            {
                // object id still exists, get it's treeviewitem container
                TreeViewItem item = GetTreeViewItem(trvPositions, selPosition);
                if (item != null)
                {
                    // container obtained
                    item.BringIntoView();
                    item.IsSelected = true;
                    item.Focus();
                }
            }

            if (CurrentEditorPosition != null)
            {
                PositionEditorRefresh();
            }            
        }

        private void Organization_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            //Upd
        }

        private void PositionEditorClear()
        {
            lsvPersonEditor_Roles.ItemsSource = null;
            lsvPositionEditor_DirectReports.ItemsSource = null;
            cmbPositionEditor_Person.ItemsSource = null;
            CurrentEditorPosition = null;
        }

        private void CurrentEditorPosition_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {

        }

        private void PositionEditorEditStart()
        {
            if (CurrentEditorPosition == null)
            {
                throw new InvalidOperationException("PositionEditorEditStart() called, but CurrentEditorPosition is null.");
            }

            try
            {
                // acquire lock
                CurrentEditorPositionLockId = Program.ConnectionMgr.LockAcquire(CurrentEditorPosition.ObjectID);

                CurrentEditorPosition.PropertyChanged += CurrentEditorPosition_PropertyChanged;

                // editing now
                IsEditing = true;

                // enable edit controls
                grdPersonEditor.IsEnabled = true;

                // start lock refresh timer
                lockUpdateTimer.Interval = new TimeSpan(0, 0, 30); // every 30 seconds
                lockUpdateTimer.Start();
            }
            catch (Exception ex)
            {
                lockUpdateTimer.Stop();

                // lock failed
                CurrentEditorPositionLockId = Guid.Empty;

                // not editing
                IsEditing = false;
                grdPersonEditor.IsEnabled = false;

                // show error
                MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }

        private void PositionEditorSave()
        {
            if (IsEditing)
            {
                Program.ConnectionMgr.PositionWrite(CurrentEditorPositionLockId, CurrentEditorPosition);
                Program.ConnectionMgr.FetchUpdates();
            }
        }

        private void PositionEditorRefresh()
        {
            lsvPositionEditor_DirectReports.ItemsSource = null;
            if (CurrentEditorPosition != null) { lsvPositionEditor_DirectReports.ItemsSource = CurrentEditorPosition.DirectReports; }
        }

        private void PositionEditorEditEnd()
        {
            if (IsEditing)
            {
                CurrentEditorPosition.PropertyChanged -= CurrentEditorPosition_PropertyChanged;

                // stop lock refresh
                lockUpdateTimer.Stop();

                // release lock
                if (CurrentEditorPositionLockId != Guid.Empty) { Program.ConnectionMgr.LockRelease(CurrentEditorPositionLockId); }
                CurrentEditorPositionLockId = Guid.Empty;

                // disable edit controls
                grdPersonEditor.IsEnabled = false;

                // no longer editing
                IsEditing = false;
            }
        }


        private void ConnectionMgr_ConnectionManagerEvent(object sender, Session.ConnectionManagerEventArgs e)
        {
            if (e.Connected)
            {
                // bind treeview
                trvPositions.ItemsSource = Program.ConnectionMgr.OrgStructureWrapped;
                
                // update UI stuff
                lblConnection.Text = "Connected to " + Program.ConnectionMgr.Organization.Name;
                mnuOrganization_Connect.Visibility = Visibility.Collapsed;
                mnuOrganization_Disconnect.Visibility = Visibility.Visible;
                mnuOrganization_ManageRolesPeopleSep.Visibility = Visibility.Visible;
                mnuOrganization_ManageRoles.Visibility = Visibility.Visible;
                mnuOrganization_ManagePeople.Visibility = Visibility.Visible;
                mnuActions_PositionAddSep.Visibility = Visibility.Visible;
                mnuOrganization_Settings.IsEnabled = false;
                mnuActions.IsEnabled = true;
                mnuOrganization_ExportToClipboard.IsEnabled = true;
                grdPersonEditor.IsEnabled = false;

                // select tv first item (organization) - remove either section and it stops working ¯\_(ツ)_/¯
                TreeViewItem item = GetTreeViewItem(trvPositions, Program.ConnectionMgr.OrgStructureWrapped);
                if (item != null)
                {
                    item.BringIntoView();
                    item.IsSelected = true;
                    item.Focus();
                }
                TreeViewItem tvi = (TreeViewItem)trvPositions.ItemContainerGenerator.ContainerFromIndex(0);
                if (tvi != null)
                {
                    tvi.BringIntoView();
                    tvi.IsSelected = true;
                    tvi.Focus();
                }
            }
            else
            {
                // unbind treeview
                trvPositions.ItemsSource = null;
                
                // update UI stuff
                lblConnection.Text = "Not Connected";            
                mnuOrganization_Connect.Visibility = Visibility.Visible;
                mnuOrganization_Disconnect.Visibility = Visibility.Collapsed;
                mnuOrganization_ManageRolesPeopleSep.Visibility = Visibility.Collapsed;
                mnuOrganization_ManageRoles.Visibility = Visibility.Collapsed;
                mnuOrganization_ManagePeople.Visibility = Visibility.Collapsed;
                mnuActions_PositionAddSep.Visibility = Visibility.Collapsed;
                mnuActions.IsEnabled = false;
                mnuOrganization_Settings.IsEnabled = true;
                grdPersonEditor.IsEnabled = false;
                mnuOrganization_ExportToClipboard.IsEnabled = false;
                PositionEditorClear();
            }
        }


        private void Main_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                
            }
        }

        private void InitializeGUI()
        {
            lblConnection.Text = "Not Connected";
            lblSession.Text = "Session ID: " + Program.SessionMgr.SessionID.ToString();
            mnuOrganization_Disconnect.Visibility = Visibility.Collapsed;
            grdPersonEditor.IsEnabled = false;
            mnuActions_PositionEdit.Visibility = Visibility.Collapsed;
            mnuActions_PositionEditCancel.Visibility = Visibility.Collapsed;
            mnuActions_PositionEditSave.Visibility = Visibility.Collapsed;           
            mnuActions_PositionRemove.Visibility = Visibility.Collapsed;
            mnuActions.IsEnabled = false;
        }

        private void mnuOrganization_Settings_Click(object sender, RoutedEventArgs e)
        {
            ServerSettings dlgSettings = new ServerSettings();
            dlgSettings.Owner = this;
            dlgSettings.ShowDialog();
            if (dlgSettings.Saved)
            {
                Properties.Settings.Default.ServerURL = ConfigurationRepository.ServerURL;
                Properties.Settings.Default.Save();
            }
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e)
        {
            var tvi = trvPositions.ItemContainerGenerator.ContainerFromItem(sender) as TreeViewItem;

            if (tvi != null)
            {

            }
        }

        private void TreeViewItem_Expanded(object sender, RoutedEventArgs e)
        {

        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

        }
       
        private void Refresh()
        {
            // try to obtain current persistable objectid
            Guid selObjectID = Guid.Empty;
            if (trvPositions.SelectedItem != null)
            {
                if (trvPositions.SelectedItem is IPersistable)
                {
                    selObjectID = ((IPersistable)trvPositions.SelectedItem).ObjectID;
                }
            }

            Program.ConnectionMgr.Refresh();

            // try restore selected item
            PositionModel selPosition = Program.ConnectionMgr.Organization.Positions.Where(p => p.ObjectID == selObjectID).FirstOrDefault();
            if (selPosition != null)
            {
                TreeViewItem item = GetTreeViewItem(trvPositions, selPosition);
                if (item != null)
                {
                    item.BringIntoView();
                    item.IsSelected = true;
                }
            }

            // refresh position editor
            PositionEditorRefresh();
        }

        private bool CheckUnsavedChanges()
        {
            if (IsEditing)
            {
                MessageBoxResult bRes = MessageBox.Show(this, "You have open edits.\n\rSave changes to position?", "OrgStructClient", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
                if (bRes == MessageBoxResult.Yes)
                {
                    // write changes to server
                    Program.ConnectionMgr.PositionWrite(CurrentEditorPositionLockId, CurrentEditorPosition);
                    CurrentEditorPosition.IsDirty = false;

                    // stop editing
                    PositionEditorEditEnd();

                    // fetch updates
                    Program.ConnectionMgr.FetchUpdates();
                    return true;
                }
                else
                {
                    // edit cancelled
                    PositionEditorEditEnd();

                    Refresh();
                    return true;

                }
            }
            return false;
        }

        private void trvPositions_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (Program.ConnectionMgr.State == ConnectionManagerState.Disconnected) {  return ;}

            if (IsRebinding) { return; }

            if (CheckUnsavedChanges()) { return; }
            
            PositionEditorClear();
            if (e.NewValue == null) { return; }

            if (e.NewValue is RootPosition)
            {   
                CurrentEditorPosition = null;
                grdPersonEditor.IsEnabled = false;
                lsvPositionEditor_DirectReports.ItemsSource = Organization.Structure;

                mnuActions_PositionAddSep.Visibility = Visibility.Collapsed;
                mnuActions_PositionEdit.Visibility = Visibility.Collapsed;
                mnuActions_PositionEditCancel.Visibility = Visibility.Collapsed;
                mnuActions_PositionEditSave.Visibility = Visibility.Collapsed;
                mnuActions_PositionRemoveSep.Visibility = Visibility.Collapsed;
                mnuActions_PositionRemove.Visibility = Visibility.Collapsed;
            }
            else if (e.NewValue is PositionModel)
            {
                grdPersonEditor.IsEnabled = false;
                CurrentEditorPosition = (PositionModel)e.NewValue;
                
                cmbPositionEditor_Person.ItemsSource = Organization.People.OrderBy(p => p.Name); ;
                cmbPositionEditor_Person.SelectedItem = CurrentEditorPosition.Person;
                lsvPersonEditor_Roles.ItemsSource = CurrentEditorPosition.Roles;
                lsvPositionEditor_DirectReports.ItemsSource = CurrentEditorPosition.DirectReports;

                mnuActions_PositionAddSep.Visibility = Visibility.Visible;
                mnuActions_PositionEdit.Visibility = Visibility.Visible;
                mnuActions_PositionEditCancel.Visibility = Visibility.Collapsed;
                mnuActions_PositionEditSave.Visibility = Visibility.Collapsed;
                mnuActions_PositionRemoveSep.Visibility = Visibility.Visible;
                mnuActions_PositionRemove.Visibility = Visibility.Visible;
            }
        }

        private void mnuOrganization_Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Program.ConnectionMgr.Connect();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.ToString(), "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void mnuOrganization_Disconnect_Click(object sender, RoutedEventArgs e)
        {
            Program.ConnectionMgr.Disconnect();
        }

        private void mnuOrganization_Exit_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentEditorPosition != null && IsEditing)
            {
                MessageBoxResult res = MessageBox.Show("You have open edits.\n\rExit anyway?", "OrgStructClient", MessageBoxButton.OKCancel, MessageBoxImage.Question);
                if (res == MessageBoxResult.Cancel)
                {
                    return;
                }
            }

            // todo: release locks

            Application.Current.Shutdown();
        }

        private void btnPositionEditor_PositionAdd_Click(object sender, RoutedEventArgs e)
        {
            NewPositionDialog dlg = new NewPositionDialog(CurrentEditorPosition);
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                CurrentEditorPosition.DirectReports.Add(dlg.Position);
            }
        }

        private void mnuActions_PositionEdit_Click(object sender, RoutedEventArgs e)
        {
            PositionEditorEditStart();
        }

        private void btnPositionEditor_PersonNew_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                // new person dialog
                PersonDialog dlg = new PersonDialog();
                dlg.ShowDialog();

                // user saved new person?
                if (dlg.Saved)
                {
                    // force client update (new person was written to server)
                    Program.ConnectionMgr.FetchUpdates();

                    // use new person on current position
                    CurrentEditorPosition.Person = dlg.Person;

                    // write position to server
                    Program.ConnectionMgr.PositionWrite(CurrentEditorPositionLockId, CurrentEditorPosition);

                    // force client update (position was written to server)
                    Program.ConnectionMgr.FetchUpdates();
                }
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }

        private void btnPositionEditor_PersonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                
                try
                {
                    Guid personLockId = Guid.Empty;
                    if (CurrentEditorPosition.Person != null)
                    {
                        // acquire lock on person
                        personLockId = Program.ConnectionMgr.LockAcquire(CurrentEditorPosition.Person.ObjectID);
                    }

                    // role dialog
                    PersonDialog dlg = new PersonDialog(CurrentEditorPosition.Person);
                    dlg.PersonLockId = personLockId;
                    dlg.ShowDialog();

                    // release person lock
                    if (personLockId != Guid.Empty)
                    {
                        Program.ConnectionMgr.LockRelease(personLockId);
                    }

                    // force client update
                    Program.ConnectionMgr.FetchUpdates();

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }


        private void btnPositionEditor_RoleNew_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                // role dialog
                RoleDialog dlg = new RoleDialog();
                dlg.ShowDialog();

                if (dlg.Saved)
                {
                    // force client update (added new role to organization)
                    Program.ConnectionMgr.FetchUpdates();

                    CurrentEditorPosition.Roles.Add(dlg.Role);
                }
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }

        private void btnPositionEditor_RoleAdd_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                // role select dialog
                SelectRoleDialog dlg = new SelectRoleDialog(CurrentEditorPosition.Roles);
                dlg.ShowDialog();

                if (dlg.Saved)
                {
                    // add selected role
                    CurrentEditorPosition.Roles.Add(dlg.Role);

                    // fetch updates
                    Program.ConnectionMgr.FetchUpdates();
                }
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }

        private void cmbPositionEditor_Person_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (IsEditing)
            {
                // update person
                CurrentEditorPosition.Person = (PersonModel)cmbPositionEditor_Person.SelectedItem;
            }
        }

        private void btnPositionEditor_RoleEdit_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                if (lsvPersonEditor_Roles.SelectedItem != null)
                {
                    Guid roleLockId = Guid.Empty;
                    try
                    {
                        // acquire lock
                        roleLockId = Program.ConnectionMgr.LockAcquire(((RoleModel)lsvPersonEditor_Roles.SelectedItem).ObjectID);

                        // role dialog
                        RoleDialog dlg = new RoleDialog((RoleModel)lsvPersonEditor_Roles.SelectedItem);
                        dlg.RoleLockId = roleLockId;
                        dlg.ShowDialog();

                        // release lock
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
                        return;
                    }
                }
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }

        private void mnuActions_PositionEditSave_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {                
                Program.ConnectionMgr.PositionWrite(CurrentEditorPositionLockId, CurrentEditorPosition);
                PositionEditorEditEnd();
                Program.ConnectionMgr.FetchUpdates();                
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }

        private void mnuActions_PositionEditCancel_Click(object sender, RoutedEventArgs e)
        {
            if (IsEditing)
            {
                PositionEditorEditEnd();
                Program.ConnectionMgr.Refresh();
            }
            else
            {
                throw new InvalidOperationException("Editor not in edit mode.");
            }
        }

        private void mnuActions_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            mnuActions_PositionEdit.IsEnabled = !IsEditing;
            mnuActions_PositionEditSave.Visibility = (IsEditing ? Visibility.Visible : Visibility.Collapsed);
            mnuActions_PositionEditCancel.Visibility = (IsEditing ? Visibility.Visible : Visibility.Collapsed);
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            
        }

        private void mnuOrganization_ManageRoles_Click(object sender, RoutedEventArgs e)
        {
            ManageRolesDialog dlg = new ManageRolesDialog();
            dlg.ShowDialog();
        }


        private void mnuOrganization_ManagePeople_Click(object sender, RoutedEventArgs e)
        {
            ManagePeopleDialog dlg = new ManagePeopleDialog();
            dlg.ShowDialog();
        }

        private void mnuActions_PositionAdd_Click(object sender, RoutedEventArgs e)
        {
            // select parent of new position
            PositionModel parentPos = null;
            if (trvPositions.SelectedItem != null)
            {
                if (!(trvPositions.SelectedItem is RootPosition))
                {
                    if (trvPositions.SelectedItem is PositionModel)
                    {
                        parentPos = (PositionModel)trvPositions.SelectedItem;
                    }
                }
            }

            // show dialog
            NewPositionDialog dlg = new NewPositionDialog(parentPos);
            dlg.ShowDialog();

            if (dlg.Saved)
            {
                // write position
                Program.ConnectionMgr.PositionWrite(Guid.Empty, dlg.Position, parentPos);
                
                // fetch updates after write
                Program.ConnectionMgr.FetchUpdates();
            }
        }

        private void mnuActions_PositionRemove_Click(object sender, RoutedEventArgs e)
        {
            if (trvPositions.SelectedItem != null)
            {
                if (!(trvPositions.SelectedItem is RootPosition))
                {                    
                    try
                    {
                        // acquire lock
                        Guid positionLockId = Program.ConnectionMgr.LockAcquire(((PositionModel)trvPositions.SelectedItem).ObjectID);

                        // delete position
                        Program.ConnectionMgr.PositionDelete(positionLockId, (PositionModel)trvPositions.SelectedItem);

                        // release lock
                        Program.ConnectionMgr.LockRelease(positionLockId);

                        // fetch updates
                        Program.ConnectionMgr.FetchUpdates();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void btnPositionEditor_PositionRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lsvPositionEditor_DirectReports.SelectedItem != null)
            {
                try
                {
                    PositionModel directReport = (PositionModel)lsvPositionEditor_DirectReports.SelectedItem;
                    // acquire lock for direct report
                    Guid drLockId = Program.ConnectionMgr.LockAcquire(directReport.ObjectID);

                    // remove direct report
                    Program.ConnectionMgr.PositionDelete(drLockId, directReport);

                    // write position
                    //Program.ConnectionMgr.PositionWrite(CurrentEditorPositionLockId, (PositionModel)lsvPositionEditor_DirectReports.SelectedItem);

                    // release direct report lock
                    Program.ConnectionMgr.LockRelease(drLockId);

                    // fetch updates
                    Program.ConnectionMgr.FetchUpdates();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "OrgStructClient", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void btnPositionEditor_RoleRemove_Click(object sender, RoutedEventArgs e)
        {
            if (lsvPersonEditor_Roles.SelectedItem != null)
            {
                // remove role from position
                CurrentEditorPosition.Roles.Remove((RoleModel)lsvPersonEditor_Roles.SelectedItem);

                // fetch updates
                Program.ConnectionMgr.FetchUpdates();
            }
        }

        private void mnuOrganization_ExportToClipboard_Click(object sender, RoutedEventArgs e)
        {
            string orgJson = JsonConvert.SerializeObject(Program.ConnectionMgr.Organization);
            Clipboard.SetText(orgJson);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            CheckUnsavedChanges();
        }


























        // lifted from https://learn.microsoft.com/en-us/dotnet/desktop/wpf/controls/how-to-find-a-treeviewitem-in-a-treeview?view=netframeworkdesktop-4.8

        /// <summary>
        /// Recursively search for an item in this subtree.
        /// </summary>
        /// <param name="container">
        /// The parent ItemsControl. This can be a TreeView or a TreeViewItem.
        /// </param>
        /// <param name="item">
        /// The item to search for.
        /// </param>
        /// <returns>
        /// The TreeViewItem that contains the specified item.
        /// </returns>
        private TreeViewItem GetTreeViewItem(ItemsControl container, object item)
        {
            if (container != null)
            {
                if (container.DataContext == item)
                {
                    return container as TreeViewItem;
                }

                // Expand the current container
                if (container is TreeViewItem && !((TreeViewItem)container).IsExpanded)
                {
                    container.SetValue(TreeViewItem.IsExpandedProperty, true);
                }

                // Try to generate the ItemsPresenter and the ItemsPanel.
                // by calling ApplyTemplate.  Note that in the
                // virtualizing case even if the item is marked
                // expanded we still need to do this step in order to
                // regenerate the visuals because they may have been virtualized away.

                container.ApplyTemplate();
                ItemsPresenter itemsPresenter =
                    (ItemsPresenter)container.Template.FindName("ItemsHost", container);
                if (itemsPresenter != null)
                {
                    itemsPresenter.ApplyTemplate();
                }
                else
                {
                    // The Tree template has not named the ItemsPresenter,
                    // so walk the descendents and find the child.
                    itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    if (itemsPresenter == null)
                    {
                        container.UpdateLayout();

                        itemsPresenter = FindVisualChild<ItemsPresenter>(container);
                    }
                }

                Panel itemsHostPanel = (Panel)VisualTreeHelper.GetChild(itemsPresenter, 0);

                // Ensure that the generator for this panel has been created.
                UIElementCollection children = itemsHostPanel.Children;

                MyVirtualizingStackPanel virtualizingPanel =
                    itemsHostPanel as MyVirtualizingStackPanel;

                for (int i = 0, count = container.Items.Count; i < count; i++)
                {
                    TreeViewItem subContainer;
                    if (virtualizingPanel != null)
                    {
                        // Bring the item into view so
                        // that the container will be generated.
                        virtualizingPanel.BringIntoView(i);

                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);
                    }
                    else
                    {
                        subContainer =
                            (TreeViewItem)container.ItemContainerGenerator.
                            ContainerFromIndex(i);

                        // Bring the item into view to maintain the
                        // same behavior as with a virtualizing panel.
                        subContainer.BringIntoView();
                    }

                    if (subContainer != null)
                    {
                        // Search the next level for the object.
                        TreeViewItem resultContainer = GetTreeViewItem(subContainer, item);
                        if (resultContainer != null)
                        {
                            return resultContainer;
                        }
                        else
                        {
                            // The object is not under this TreeViewItem
                            // so collapse it.
                            subContainer.IsExpanded = false;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Search for an element of a certain type in the visual tree.
        /// </summary>
        /// <typeparam name="T">The type of element to find.</typeparam>
        /// <param name="visual">The parent element.</param>
        /// <returns></returns>
        private T FindVisualChild<T>(Visual visual) where T : Visual
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(visual); i++)
            {
                Visual child = (Visual)VisualTreeHelper.GetChild(visual, i);
                if (child != null)
                {
                    T correctlyTyped = child as T;
                    if (correctlyTyped != null)
                    {
                        return correctlyTyped;
                    }

                    T descendent = FindVisualChild<T>(child);
                    if (descendent != null)
                    {
                        return descendent;
                    }
                }
            }

            return null;
        }
    }
}
