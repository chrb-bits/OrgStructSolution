using Newtonsoft.Json;
using OrgStructLogic.Service;
using OrgStructModels.Protocol;
using System;
using System.Net.Http;
using System.Text;

namespace OrgStructServerOWINAPI
{
    public static class Testing
    {
        public static void Test(string baseAddress)
        {
            // Create HttpClient and make a request to api/values 
            using (var client = new HttpClient())
            {
                var reqOrgRead = new OrganizationRequest();

                // GET 
                //var response = client.GetAsync(baseAddress + "organization/read").Result;

                var content = new StringContent(JsonConvert.SerializeObject(Facilities.PersistenceLayer), Encoding.UTF8, "application/json");
                var response = client.PostAsync(baseAddress + "organization/read", content).Result;

                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
            }
        }
    }
}
