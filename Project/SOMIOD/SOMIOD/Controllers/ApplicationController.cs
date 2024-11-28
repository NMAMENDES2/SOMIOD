using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Text;
using SOMIOD.Models;
using System.Web.Services.Description;
using System.Xml.Serialization;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ApplicationController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;

        [Route("")]
        [HttpGet]

        // Terceiro endpoints dado no enunciado localhost/api/somiod
        public HttpResponseMessage GetAllApplication() // Não dá com HTTPActionResult tem de ser assim
        {
            var apps = new List<Application>();

            HttpResponseMessage response;
            var headers = HttpContext.Current.Request.Headers;

            string somiodDiscover = headers.Get("somiod-locate"); // Meter somiod-discover nos headers no postman com value application

            // Fetch db
            if (somiodDiscover == "application")
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Application";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader registos = cmd.ExecuteReader();
                    while (registos.Read())
                    {
                        Application app = new Application();
                        app.name = (string)registos["name"];
                        apps.Add(app);
                    }
                }
            }
            // Cria um xml, dá-lhe declaração e cria os nodes
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("response");
            doc.AppendChild(root);
            foreach (Application app in apps)
            {
                XmlElement application = doc.CreateElement("application");
                XmlElement name = doc.CreateElement("name");
                name.InnerText = app.name;
                application.AppendChild(name);
                root.AppendChild(application);
            }

            string xmlContent = doc.OuterXml;
            response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

            return response;
        }
      
    }
}