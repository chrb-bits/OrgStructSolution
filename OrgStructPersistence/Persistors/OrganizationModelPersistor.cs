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
        private Exception Ex_CantWrite = new IOException("Can't write to JSON persistence file. Persistence layer not connected?");
        private Exception Ex_CantRead = new IOException("Can't read from JSON persistence file. Persistence layer not connected?");

        // private functions to manipulate persistence/dirty flags
        private void SetPersistenceFlags(OrganizationModel organizationModel, bool? isPersistent, bool? isDirty)
        {
            // roles
            foreach (RoleModel role in organizationModel.Roles)
            {
                if (isPersistent != null)
                {
                    //role.IsPersistent = isPersistent ?? default(bool);
                    SetIsPersistent(role, isPersistent ?? default);
                }
                if (isDirty != null)
                {
                    //role.IsDirty = isDirty ?? default(bool);
                    SetIsDirty(role, isDirty ?? default);
                }
            }

            foreach (PersonModel person in organizationModel.People)
            {
                if (isPersistent != null)
                {
                    //person.IsPersistent = isPersistent ?? default(bool);
                    SetIsPersistent(person, isPersistent ?? default);
                }
                if (isDirty != null)
                {
                    //person.IsDirty = isDirty ?? default(bool);
                    SetIsDirty(person, isDirty ?? default);
                }
            }
            
            foreach (PositionModel position in organizationModel.Structure)
            {
                SetPersistenceFlags(position, isPersistent, isDirty);
            }
        }

        private void SetPersistenceFlags(PositionModel position, bool? isPersistent, bool? isDirty)
        {
            foreach (PositionModel directReport in position.DirectReports)
            {
                if (isPersistent != null) { SetIsPersistent(directReport, isPersistent ?? default); } //  directReport.IsPersistent = isPersistent ?? default; }
                if (isDirty != null) { SetIsDirty(directReport, isPersistent ?? default); } // directReport.IsDirty = isDirty ?? default; }
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

        public override OrganizationModel Read(string key = "")
        {
            // only works if we can read from the JSON persistence stream
            if (!((PersistenceLayer)persistence).PersistenceFileStream.CanRead) { throw Ex_CantRead; }

            // create streamreader on persistence filestream            
            using (StreamReader rdr = new NoCloseStreamReader(((PersistenceLayer)persistence).PersistenceFileStream))
            {
                // read json data from stream position 0 into string
                rdr.BaseStream.Seek(0, SeekOrigin.Begin);
                string jsonData = rdr.ReadToEnd();

                // deserialize
                OrganizationModel organization = JsonConvert.DeserializeObject<OrganizationModel>(jsonData, new JsonSerializerSettings { PreserveReferencesHandling = PreserveReferencesHandling.Objects, ReferenceResolverProvider = () => new PersistableReferenceResolver() });

                // update persistence flags
                SetPersistenceFlags(organization, true, false);

                // return as IPersistable
                return organization;
            }
        }

        public override void Write(OrganizationModel persistable)
        {
            // only works if we can write to the JSON persistence file stream
            if (!((PersistenceLayer)persistence).PersistenceFileStream.CanWrite) { throw Ex_CantWrite; }

            // create streamwriter on persistence filestream
            using (StreamWriter writer = new NoCloseStreamWriter(((PersistenceLayer)persistence).PersistenceFileStream))
            {
                // serialize organization peristable
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

        public override void Delete(OrganizationModel persistable)
        {
            // TODO: doesn't work anymore, but it's never used anyways.
            throw new NotImplementedException("OrganizationModelPersistor.Delete() not currently implemented.");
            
            string jsonFilePath = ((PersistenceLayer)persistence).PersistenceFilePath;
            if (File.Exists(jsonFilePath))
            {
                File.Delete(jsonFilePath);
            }

            // update persistence flags
            SetPersistenceFlags(persistable, false, null);
        }
        #endregion
    }
}
