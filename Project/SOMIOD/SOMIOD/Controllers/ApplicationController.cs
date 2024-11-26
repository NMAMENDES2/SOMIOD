using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using SOMIOD.Models;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ApplicationController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;

        [Route("app")]
        [HttpGet]
        public IEnumerable<Application> getApp() {
            var apps = new List<Application>();
            using (SqlConnection connection = new SqlConnection(connstr))
            {
                try
                {
                    connection.Open();
                    string query = "DELETE * FROM Application";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader registos = cmd.ExecuteReader();

                    while (registos.Read()) {
                        Application app = new Application
                        {
                            id = (int)registos["Id"],
                            name = (string)registos["name"],
                            creation_datetime = (DateTime)registos["creation_datetime"],
                        };

                        // Tá a devolver array vazio. Isto dps acho que se tem de mandar em xml
                        apps.Add(app);
                    }

                    registos.Close();
                    connection.Close();
                    return apps;
                }
                catch (Exception ex)
                {
                    if (connection.State == System.Data.ConnectionState.Open) {
                        connection.Close();
                    }
                }

                return apps;
            }
        }
    }
}
