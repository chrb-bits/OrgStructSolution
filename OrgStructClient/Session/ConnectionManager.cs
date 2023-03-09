using Newtonsoft.Json;
using OrgStructModels.Metadata;
using OrgStructModels.Persistables;
using OrgStructModels.Persistence;
using OrgStructModels.Protocol;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace OrgStructClient.Session
{
    public delegate void ConnectionManagerEventHandler(object sender, ConnectionManagerEventArgs e);

    public class ConnectionManager
    {
        private DispatcherTimer updateTimer;

        private HttpClient client;

        private Exception Ex_NoConnection = new IOException("Not connected.");

        public ConnectionManager()
        {
            updateTimer = new DispatcherTimer();
            client = new HttpClient();
        }

        ~ConnectionManager()
        {
            Disconnect();

            if (updateTimer != null)
            {
                updateTimer.Stop();
                updateTimer.Tick -= UpdateTimer_Tick;
                updateTimer = null;
            }

            if (client != null)
            {
                client.CancelPendingRequests();
                client.Dispose();
                client = null;
            }
            

        }

        public event ConnectionManagerEventHandler ConnectionManagerEvent;

        public event EventHandler Refreshed;

        public event EventHandler Updated;

        public Guid SessionID { set; get; }

        public ConnectionManagerState State { set; get; }

        public OrganizationModel Organization { private set; get; }

        public RootPosition OrgStructureWrapped
        {
            get
            {
                // wrap into dummy root/org objects for WPF treeview display
                RootPosition org = new RootPosition();
                org.DirectReports = Organization.Structure;
                org.OrgName = Organization.Name;
                RootPosition root = new RootPosition();
                root.DirectReports.Add(org);
                return root;
            }
        }
        public DateTime LastUpdatedUTC { private set; get; }

        public void Connect()
        {
            updateTimer.Stop();

            if (State != ConnectionManagerState.Disconnected)
            {                
                Disconnect();
            }

            State = ConnectionManagerState.Connecting;

            // read org
            if (Organization != null) { Organization.PropertyChanged -= Organization_PropertyChanged; }
            Organization = OrganizationRead();
            if (Organization != null) { Organization.PropertyChanged += Organization_PropertyChanged; }

            // timestamp
            LastUpdatedUTC = DateTime.UtcNow;

            // success?
            if (Organization != null)
            {
                State = ConnectionManagerState.Connected;
                ConnectionManagerEvent?.Invoke(this, new ConnectionManagerEventArgs(true, "Connected to organization (" + Organization.Name + ")."));
                updateTimer.Tick += UpdateTimer_Tick;
                ResetUpdateTimer();
                updateTimer.Start();
            }
            else
            {
                updateTimer.Stop();
                Disconnect();
            }
        }

        private void ResetUpdateTimer()
        {
            updateTimer.Interval = new TimeSpan(0, 0, 3);
        }


        private void UpdateTimer_Tick(object sender, EventArgs e)
        {
            if (State == ConnectionManagerState.Connected)
            {
                System.Diagnostics.Debug.WriteLine("PERIODIC UPDATE");
                try
                {
                    FetchUpdates();
                }
                catch (Exception ex)
                {                    
                    Disconnect();
                    // todo: error event
                }
            }
            else
            {
                updateTimer.Tick -= UpdateTimer_Tick;
                updateTimer.Stop();
            }
        }

        private void Organization_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("PropertyChanged event (Source=" + sender.ToString() + ", Property=" + e.PropertyName + ").");
        }

        private RoleModel GetLocalRole(RoleModel updateRole)
        {
            return Organization.Roles.Where(x => x.ObjectID == updateRole.ObjectID).FirstOrDefault();
        }

        private RoleModel GetUpdateRole(RoleModel localRole, List<RoleModel> updateRoles)
        {
            return updateRoles.Where(x => x.ObjectID == localRole.ObjectID).FirstOrDefault();
        }

        public void Disconnect()
        {
            if (State == ConnectionManagerState.Connected)
            {
                // unhook update timer event
                updateTimer.Tick -= UpdateTimer_Tick;

                // release all server side locks
                try
                {
                    LockReleaseAll();
                }
                catch (AggregateException ex)
                {
                    // ignore it
                }

                // raise disconnected event
                ConnectionManagerEvent?.Invoke(this, new ConnectionManagerEventArgs(false, "Disconnected from organization (" + Organization.Name + ")."));
                
            }
            State = ConnectionManagerState.Disconnected;

            Organization = null;
        }


        public void Refresh()
        {
            if (State == ConnectionManagerState.Connected)
            {
                // read full org tree from server
                Organization = null;
                Organization = OrganizationRead();
                
                // we just updated
                LastUpdatedUTC = DateTime.UtcNow;
                
                // silly notification event to work around strange issues with treeview databinding
                Refreshed?.Invoke(this, new EventArgs());
            }
            else
            {
                throw new IOException("No connection.");
            }
        }

        bool bFetchUpdatesRunning = false;
        public void FetchUpdates()
        {
            if (bFetchUpdatesRunning) { return; }
            bFetchUpdatesRunning = true;

            // fetch updates only from server
            if (State == ConnectionManagerState.Connected)
            {
                // post updatesrequest to updates/read endpoint
                UpdatesRequest req = new UpdatesRequest();
                req.ChangesFromUTC = LastUpdatedUTC;
                req.SessionID = SessionID;
                StringContent content = SerializeRequestToContent(req);
                HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "updates/read", content).Result;
                
                // read response
                string responseContent = response.Content.ReadAsStringAsync().Result;
                UpdatesResult result = DeserializeContentToResponse<UpdatesResult>(responseContent);

                if (result.Success)
                {
                    if (result.RefreshRequired)
                    {
                        // no updates, full refresh required
                        Refresh();
                    }
                    else
                    {
                        // apply received updates to local orgtree
                        ApplyUpdates(result);

                        // timestamp
                        LastUpdatedUTC = DateTime.UtcNow;
                    }
                }
                else
                {
                    bFetchUpdatesRunning = false;
                    throw new IOException("Update() failed: Server returned failure (" + result.Message + ").");
                }
            }
            else
            {
                bFetchUpdatesRunning = false;
                throw new IOException("Update() failed: No connection.");
            }

            // no longer fetching updates
            bFetchUpdatesRunning = false;

            Updated?.Invoke(this, new EventArgs());
        }

        // update local orgtree with updates from the server
        private void ApplyUpdates(UpdatesResult result)
        {
            foreach (PersonModel updatePerson in result.People)
            {
                // find in local list
                PersonModel existingPerson = Organization.People.Where(x => x.ObjectID == updatePerson.ObjectID).FirstOrDefault();

                // exists?
                if (existingPerson != null)
                {
                    // yes, update
                    updatePerson.CopyInto(existingPerson);

                    //existingPerson.Name = updatePerson.Name;
                    //existingPerson.FirstName = updatePerson.FirstName;
                }
                else
                {
                    // no, add
                    Organization.People.Add(updatePerson);
                }
            }


            // add/edit organization roles
            foreach (RoleModel updateRole in result.Roles)
            {
                // find in local list
                RoleModel existingRole = Organization.Roles.Where(x => x.ObjectID == updateRole.ObjectID).FirstOrDefault();

                // exists?
                if (existingRole != null)
                {
                    // yes, update
                    updateRole.CopyInto(existingRole);

                    //existingRole.Name = updateRole.Name;
                }
                else
                {
                    // no, add
                    Organization.Roles.Add(updateRole);
                }
            }


            foreach (PositionModel updatePosition in result.Positions)
            {
                // find in list
                PositionModel existingPos = Organization.Positions.Where(x => x.ObjectID == updatePosition.ObjectID).FirstOrDefault();

                // exists?
                if (existingPos != null)
                {
                    // yes, update
                    updatePosition.CopyInto(existingPos);

                    //existingPos.Person = Organization.People.Where(x => x.ObjectID == updatePosition.Person.ObjectID).FirstOrDefault();
                    //existingPos.Person.Name = updatePosition.Person.Name;
                    //existingPos.Person.FirstName = updatePosition.Person.FirstName;

                    // update roles
                   // UpdatePositionRoles(existingPos, updatePosition.Roles.ToList());

                    // update direct reports
                    //existingPos.DirectReports = updatePosition.DirectReports;
                }
                else
                {
                    // never happens
                    System.Diagnostics.Debug.Assert(false == true);
                }
            }
        }

        private void UpdatePositionRoles(PositionModel positionToUpdateRolesIn, List<RoleModel> updateRolesList)
        {
            // updates/adds
            foreach (RoleModel updateRole in updateRolesList)
            {
                RoleModel localRole = GetLocalRole(updateRole);
                if (localRole != null)
                {
                    // exists, update
                    localRole.Name = updateRole.Name;
                }
                else
                {
                    // does not exist, add
                    positionToUpdateRolesIn.Roles.Add(updateRole);
                }
            }

            // removals
            foreach (RoleModel localRole in positionToUpdateRolesIn.Roles)
            {
                // RoleModel updateRole = updateRolesList.Where(x => x.ObjectID == localRole.ObjectID).FirstOrDefault();
                RoleModel updateRole = GetUpdateRole(localRole, updateRolesList);
                if (updateRole == null)
                {
                    // no longer exists, remove
                    positionToUpdateRolesIn.Roles.Remove(localRole);
                }
            }
        }

        private void GetPositionsFlat(PositionModel startingPosition, List<PositionModel> positionsList)
        {
            foreach (PositionModel directReport in startingPosition.DirectReports)
            {
                // add
                positionsList.Add(directReport);

                // find descendants
                if (directReport.DirectReports.Count > 0)
                {
                    GetPositionsFlat(directReport, positionsList);
                }
            }
        }

        private OrganizationModel OrganizationRead()
        {
            // state "connecting" is allowed here
            if (State == ConnectionManagerState.Disconnected || State == ConnectionManagerState.Undefined) { throw Ex_NoConnection; }

            // send organizationrequest to org/read endpoint
            OrganizationRequest req = new OrganizationRequest();
            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "organization/read", content).Result;

            // get organizationresult
            string responseContent = response.Content.ReadAsStringAsync().Result;
            //OrganizationResult result = JsonConvert.DeserializeObject<OrganizationResult>(responseContent);
            OrganizationResult result = DeserializeContentToResponse<OrganizationResult>(responseContent);
                

            // process result
            if (result.Success)
            {
                if (result.Organization != null)
                {
                    return result.Organization;
                }
                else
                {
                    throw new IOException("Server returned null organization.");
                }
            }
            else
            {
                throw new IOException("OrganizationRead() returned failure (" + result.Message + ").");
            }
        }


        private StringContent SerializeRequestToContent(IRequest req)
        {
            string json = JsonConvert.SerializeObject(req, Formatting.Indented, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceResolverProvider = () => new PersistableReferenceResolver() });
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
            return content;
        }

        private T DeserializeContentToResponse<T>(string responseContent)
        {
            return JsonConvert.DeserializeObject<T>(responseContent, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceResolverProvider = () => new PersistableReferenceResolver() });
        }

        public void PersonWrite(Guid objectLock, PersonModel person)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            // send organizationrequest to org/read endpoint
            PersonRequest req = new PersonRequest();
            req.Person = person;
            req.SessionID = SessionID;
            req.OperationLockID = objectLock;

            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "people/write", content).Result;

            // get personresult
            string responseContent = response.Content.ReadAsStringAsync().Result;
            //PersonResult result = JsonConvert.DeserializeObject<PersonResult>(responseContent);
            PersonResult result = DeserializeContentToResponse<PersonResult>(responseContent);

            // process result
            if (!result.Success)
            {
                throw new IOException("PersonWrite() returned failure (" + result.Message + ").");
            }
        }

        public void PersonDelete(Guid objectLock, PersonModel person)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            // send organizationrequest to org/read endpoint
            PersonRequest req = new PersonRequest();
            req.Person = person;
            req.SessionID = SessionID;
            req.OperationLockID = objectLock;

            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "people/delete", content).Result;

            // get result
            string responseContent = response.Content.ReadAsStringAsync().Result;
            PersonResult result = DeserializeContentToResponse<PersonResult>(responseContent);

            // process result
            if (!result.Success)
            {
                throw new IOException("PersonDelete() failed (" + result.Message + ").");
            }
        }

        public void RoleWrite(Guid objectLock, RoleModel role)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            // send organizationrequest to org/read endpoint
            RoleRequest req = new RoleRequest();
            req.Role = role;
            req.SessionID = SessionID;
            req.OperationLockID = objectLock;
            
            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "roles/write", content).Result;

            // get personresult
            string responseContent = response.Content.ReadAsStringAsync().Result;
            RoleResult result = DeserializeContentToResponse<RoleResult>(responseContent);

            // process result
            if (!result.Success)
            {
                throw new IOException("RoleWrite() returned failure (" + result.Message + ").");
            }            
        }

        public void RoleDelete(Guid objectLock, RoleModel role)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            // send organizationrequest to org/read endpoint
            RoleRequest req = new RoleRequest();
            req.Role = role;
            req.SessionID = SessionID;
            req.OperationLockID = objectLock;

            // get result
            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "roles/delete", content).Result;

            // get personresult
            string responseContent = response.Content.ReadAsStringAsync().Result;
            RoleResult result = DeserializeContentToResponse<RoleResult>(responseContent);

            // process result
            if (!result.Success)
            {
                throw new IOException("RoleDelete() failed (" + result.Message + ").");
            }
        }

        public void PositionWrite(Guid objectLock, PositionModel position, PositionModel parentPosition = null)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            // position request
            PositionRequest req = new PositionRequest()
            {
                Position = position,
                Parent = parentPosition,
                SessionID = SessionID,
                OperationLockID = objectLock
            };

            // send request
            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "positions/write", content).Result;

            // get result
            string responseContent = response.Content.ReadAsStringAsync().Result;
            PositionResult result = DeserializeContentToResponse<PositionResult>(responseContent);

            // process result
            if (!result.Success)
            {
                throw new IOException("PositionWrite() failed (" + result.Message + ").");
            }
        }

        public void PositionDelete(Guid objectLock, PositionModel position, PositionModel parentPosition = null)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            // send position request to positions/delete endpoint
            PositionRequest req = new PositionRequest();
            req.Position = position;
            req.Parent = parentPosition;
            req.SessionID = SessionID;
            req.OperationLockID = objectLock;

            // send request
            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "positions/delete", content).Result;

            // get result
            string responseContent = response.Content.ReadAsStringAsync().Result;
            PositionResult result = DeserializeContentToResponse<PositionResult>(responseContent);

            // process result
            if (!result.Success)
            {
                throw new IOException("PositionDelete() failed (" + result.Message + ").");
            }
        }

        public Guid LockAcquire(Guid objectID)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            LockRequest req = new LockRequest();
            req.SessionID = SessionID;
            req.TargetObjectID = objectID;

            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "locks/acquire", content).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            LockResult result = DeserializeContentToResponse<LockResult>(responseContent);

            if (!result.Success)
            {
                throw new UnauthorizedAccessException("Failed to acquire object lock (" + result.Message + ").");
            }

            return result.LockID;
        }

        public void LockRelease(Guid lockID)
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            LockRequest req = new LockRequest();
            req.SessionID = SessionID;
            req.OperationLockID = lockID;

            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "locks/release", content).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            LockResult result = DeserializeContentToResponse<LockResult>(responseContent);
        }

        public void LockReleaseAll()
        {
            if (State != ConnectionManagerState.Connected) { throw Ex_NoConnection; }

            LockRequest req = new LockRequest();
            req.SessionID = SessionID;

            StringContent content = SerializeRequestToContent(req);
            HttpResponseMessage response = client.PostAsync(ConfigurationRepository.ServerURL + "locks/releaseall", content).Result;

            string responseContent = response.Content.ReadAsStringAsync().Result;
            LockResult result = DeserializeContentToResponse<LockResult>(responseContent);
        }
    }

    public enum ConnectionManagerState
    {
        Undefined = 0,
        Disconnected = 1,
        Connecting = 2,
        Connected = 3
    }

    public class ConnectionManagerEventArgs : EventArgs
    {
        public ConnectionManagerEventArgs(bool connected, string message) : base()
        {
            Connected = connected;
            Message = message;
        }

        public bool Connected { private set; get; }

        public string Message { private set; get; }
    }
}
