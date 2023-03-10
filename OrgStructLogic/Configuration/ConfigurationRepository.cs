using Newtonsoft.Json;
using System;
using OrgStructModels.Metadata;
using System.IO;

namespace OrgStructLogic.Configuration
{
    public class ConfigurationRepository
    {
        #region ctor
        public ConfigurationRepository()
        {
            Service = new ServiceConfiguration();
            Persistence = new PersistenceConfiguration();
        }
        #endregion

        #region Configuration Properties        
        /// <summary>
        /// Service configuration settings.
        /// </summary>
        public ServiceConfiguration Service { private set; get; }

        /// <summary>
        /// Persistence layer configuration settings.
        /// </summary>
        public PersistenceConfiguration Persistence { private set; get; }
        #endregion

        #region Public Interface
        /// <summary>
        /// Configuration repository log message event.
        /// </summary>
        public event EventHandler<LogEventArgs> LogEvent;

        /// <summary>
        /// Save current configuration to JSON file.
        /// </summary>
        /// <param name="configFilePath">Path of the JSON file.</param>
        public void Save(string configFilePath)
        {
            string jsonData = JsonConvert.SerializeObject(this, new JsonSerializerSettings() { Formatting = Formatting.Indented });
            File.WriteAllText(configFilePath, jsonData);
            LogEvent?.Invoke(this, new LogEventArgs("Configuration saved to (" + configFilePath + ")."));
        }

        /// <summary>
        /// Load configuration from JSON file as current.
        /// </summary>
        /// <param name="configFilePath"></param>
        public void Load(string configFilePath)
        {
            string jsonData = File.ReadAllText(configFilePath);
            JsonConvert.PopulateObject(jsonData, this, new JsonSerializerSettings() { ObjectCreationHandling = ObjectCreationHandling.Replace });
            LogEvent?.Invoke(this, new LogEventArgs("Configuration loaded from (" + configFilePath + ")."));
        }
        #endregion
    }
}
