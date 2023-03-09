using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OrgStructModels.Persistence;
using Owin;
using System.Collections.Generic;
using System.Net.Http.Formatting;
using System.Web.Http;

namespace OrgStructServerOWINAPI
{
    public class Startup
    {
        // This code configures Web API. The Startup class is specified as a type
        // parameter in the WebApp.Start method.
        public void Configuration(IAppBuilder appBuilder)
        {
            // new json serializer settings
            var defaultSettings = new JsonSerializerSettings
            {
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                ReferenceResolverProvider = () => new PersistableReferenceResolver(),
                Formatting = Formatting.Indented,
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Converters = new List<JsonConverter>
                {
                    new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() }
                }
            };
            JsonConvert.DefaultSettings = () => { return defaultSettings; };

            // Configure Web API for self-host. 
            HttpConfiguration config = new HttpConfiguration();
            _ = config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "{controller}/{action}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );

            // use json.net as webapi serdes
            config.Formatters.Clear();
            config.Formatters.Add(new JsonMediaTypeFormatter());
            config.Formatters.JsonFormatter.SerializerSettings = defaultSettings;

            appBuilder.UseWebApi(config);
        }
    }
}
