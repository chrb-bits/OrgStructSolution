using Microsoft.Owin.Hosting;
using OrgStructLogic.Service;
using OrgStructModels.Persistables;
using System;

namespace OrgStructServerOWINAPI
{
    class Program
    {
        static void Main(string[] args)
        {
            Log("OrgStructServerOWINAPI - OWIN Host for WebAPI 2 Service v0.1 - Not for resale!");
            
            // service facilities
            Log("Starting service facilities...");
            Facilities.LogEvent += Facilities_LogEvent;
            Facilities.Start();

            if (Facilities.Running)
            {
                // Start OWIN host 
                Log("Starting OWIN WebAPI host at (" + Facilities.Configuration.Service.ServiceURL + ")...");
                using (WebApp.Start<Startup>(url: Facilities.Configuration.Service.ServiceURL))
                {
                    //Log("Running dev stuff...");
                    //Dev2();

                    //Log("Test...");
                    //Testing.Test(baseAddress);

                    Log("Server running. Any key to shut down.");
                    Console.ReadKey();
                }
            }
            else
            {
                Log("ERROR: Facilities failed to start.");
                Console.ReadKey();
            }

            Log("Shutdown...");
            Facilities.Stop();
            System.Threading.Thread.Sleep(1000);
            Facilities.LogEvent -= Facilities_LogEvent;
        }

        private static void Facilities_LogEvent(object sender, OrgStructModels.Metadata.LogEventArgs e)
        {
            Log(e.Message);
        }


        // print log message to console
        private static void Log(string message)
        {
            Console.WriteLine(DateTime.UtcNow + ": " + message);
        }

        private static void Dev2()
        {            
            // create organization
            Dev(Facilities.PersistenceLayer.Organization);
            
            // sync persist storage
            Facilities.PersistenceLayer.Sync();
        }

        private static void Dev(OrganizationModel organization)
        {
            RoleModel ceoRole = new RoleModel();
            ceoRole.Name = "CEO";
            organization.Roles.Add(ceoRole);

            RoleModel hrRole = new RoleModel();
            hrRole.Name = "HR";
            organization.Roles.Add(hrRole);

            RoleModel cfoRole = new RoleModel();
            cfoRole.Name = "CFO";
            organization.Roles.Add(cfoRole);

            PersonModel ceoPerson = new PersonModel();
            ceoPerson.FirstName = "Mike";
            ceoPerson.Name = "Brenner";
            organization.People.Add(ceoPerson);

            PersonModel cfoPerson = new PersonModel();
            cfoPerson.FirstName = "Harald";
            cfoPerson.Name = "Lohrer";
            organization.People.Add(cfoPerson);

            PersonModel hrPerson = new PersonModel();
            hrPerson.FirstName = "Peter";
            hrPerson.Name = "Hediger";
            organization.People.Add(hrPerson);

            PositionModel pos = new PositionModel();
            pos.Person = ceoPerson;
            pos.Roles.Add(ceoRole);
            organization.Structure.Add(pos);

            pos = new PositionModel();
            pos.Person = cfoPerson;
            pos.Roles.Add(cfoRole);
            organization.Structure.Add(pos);

            RoleModel wrkRole = new RoleModel();
            wrkRole.Name = "Worker";
            organization.Roles.Add(wrkRole);

            PersonModel wrkPerson1 = new PersonModel();
            wrkPerson1.FirstName = "Leeroy";
            wrkPerson1.Name = "Jenkins";
            organization.People.Add(wrkPerson1);

            PositionModel wrkPos1 = new PositionModel();
            wrkPos1.Person = wrkPerson1;
            wrkPos1.Roles.Add(wrkRole);

            pos = new PositionModel();
            pos.Person = hrPerson;
            pos.Roles.Add(hrRole);
            pos.DirectReports.Add(wrkPos1);
            organization.Structure.Add(pos);
        }
    }
}
