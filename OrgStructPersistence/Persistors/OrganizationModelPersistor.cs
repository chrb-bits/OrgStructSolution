using Newtonsoft.Json;
using OrgStructModels.Persistables;
using OrgStructModels.Persistence;
using OrgStructPersistence.StreamIO;
using System;
using System.IO;

namespace OrgStructPersistence.Persistors
{
    /// <summary>
    /// JSON Persistor for OgStructModels.Persistables.OrganizationModel
    /// </summary>
    public class OrganizationModelPersistor : APersistorBase<OrganizationModel>
    {
        #region Constructor
        public OrganizationModelPersistor(PersistenceLayer persistence) : base(persistence) { }
        #endregion

        #region Privates
        // exceptions we can throw
        private Exception Ex_CantWrite = new IOException("Can't write to JSON persistence file. Persistence layer not connected?");
        private Exception Ex_CantRead = new IOException("Can't read from JSON persistence file. Persistence layer not connected?");

        // private functions to manipulate persistence/dirty flags
        private void SetPersistenceFlags(OrganizationModel organizationModel, bool? isPersistent, bool? isDirty)
        {
            // roles
            foreach (RoleModel role in organizationModel.Roles)
            {
                if (isPersistent != null) { SetIsPersistent(role, isPersistent ?? default); }
                if (isDirty != null) { SetIsDirty(role, isDirty ?? default); }
            }

            foreach (PersonModel person in organizationModel.People)
            {
                if (isPersistent != null) { SetIsPersistent(person, isPersistent ?? default); }
                if (isDirty != null) { SetIsDirty(person, isDirty ?? default); }
            }
            
            // avoid recursion (if positions list is cached)
            foreach (PositionModel position in organizationModel.Positions)
            {
                if (isPersistent != null) { SetIsPersistent(position, isPersistent ?? default); }
                if (isDirty != null) { SetIsDirty(position, isDirty ?? default); }
            }
            
            //foreach (PositionModel position in organizationModel.Structure)
            //{
            //    SetPersistenceFlags(position, isPersistent, isDirty);
            //}
        }

        private void SetPersistenceFlags(PositionModel position, bool? isPersistent, bool? isDirty)
        {
            foreach (PositionModel directReport in position.DirectReports)
            {
                if (isPersistent != null) { SetIsPersistent(directReport, isPersistent ?? default); }
                if (isDirty != null) { SetIsDirty(directReport, isPersistent ?? default); }
                
                // recursion
                SetPersistenceFlags(directReport, isPersistent, isDirty);
            }
        }
        #endregion

        #region Public Interface
        public bool SupportsType<T>()
            where T : IPersistable
        {
            if (typeof(T) == typeof(OrganizationModel)) { return true; }
            return false;
        }

        /// <summary>
        /// Read OrganizationModel instance from JSON persistence file.
        /// </summary>
        /// <param name="objectID">The objectID of the OrganizationModel instance to read (ignored by OrgStructPersistenceJSON).</param>
        /// <returns>The Persistable as read from the JSON persistence file.</returns>
        public override OrganizationModel Read(Guid objectID = default)
        {
            // only works if we can read from the JSON persistence stream
            if (!((PersistenceLayer)persistence).PersistenceFileStream.CanRead) { throw Ex_CantRead; }

            // create streamreader on persistence filestream            
            using (StreamReader reader = new NoCloseStreamReader(((PersistenceLayer)persistence).PersistenceFileStream))
            {
                // read json data from stream position 0 into string
                reader.BaseStream.Seek(0, SeekOrigin.Begin);
                string jsonData = reader.ReadToEnd();

                // deserialize
                OrganizationModel organization = JsonConvert.DeserializeObject<OrganizationModel>(jsonData, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceResolverProvider = () => new PersistableReferenceResolver() });

                // update persistence flags
                SetPersistenceFlags(organization, true, false);

                // return as IPersistable
                return organization;
            }
        }

        /// <summary>
        /// Write OrganizationModel instance to JSON persistence file.
        /// </summary>
        /// <param name="persistable">The OrganizationModel instance to write.</param>
        public override void Write(OrganizationModel persistable)
        {
            // only works if we can write to the JSON persistence file stream
            if (!((PersistenceLayer)persistence).PersistenceFileStream.CanWrite) { throw Ex_CantWrite; }

            // create streamwriter on persistence filestream
            using (StreamWriter writer = new NoCloseStreamWriter(((PersistenceLayer)persistence).PersistenceFileStream))
            {
                // serialize organization persistable
                string jsonData = JsonConvert.SerializeObject(persistable, Formatting.Indented,
                    new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceResolverProvider = () => new PersistableReferenceResolver() });

                // completely overwrite persistence file stream with serialized json data
                writer.BaseStream.SetLength(0);
                writer.Write(jsonData);
                writer.Flush();
            }

            // update persistence flags
            SetPersistenceFlags(persistable, true, false);
        }

        /// <summary>
        /// Delete the Persistable from the JSON persistence file.
        /// </summary>
        /// <param name="persistable">The Persistable to delete (ignored by OrgStructPersistenceJSON).</param>
        public override void Delete(OrganizationModel persistable)
        {
            // only works if we can write to the JSON persistence file stream
            if (!((PersistenceLayer)persistence).PersistenceFileStream.CanWrite) { throw Ex_CantWrite; }

            // create streamwriter on persistence filestream
            using (StreamWriter writer = new NoCloseStreamWriter(((PersistenceLayer)persistence).PersistenceFileStream))
            {
                // serialize empty OrganizationModel to json
                string jsonData = JsonConvert.SerializeObject(new OrganizationModel(), Formatting.Indented,
                    new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceResolverProvider = () => new PersistableReferenceResolver() });

                // completely overwrite persistence file stream with serialized json data
                writer.BaseStream.SetLength(0);
                writer.Write(jsonData);
                writer.Flush();
            }

            // update persistence flags
            SetPersistenceFlags(persistable, false, null);
        }
        #endregion
    }
}
