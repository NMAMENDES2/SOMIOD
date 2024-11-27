using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;

using SOMIOD.Models;

namespace SOMIOD.Controllers
{

    [RoutePrefix("api/somiod")]
    public class ContainerController : ApiController
    {

        string connStr = Properties.Settings.Default.ConString;
        List<Container> cont = new List<Container>();

        [HttpGet]
        [Route("{application}")]
        public HttpResponseMessage GetAllContainers(string application) // Não dá com HTTPActionResult tem de ser assim
        {

            HttpResponseMessage response;
            Application app = new Application();
            // Fetch db
            using (SqlConnection connection = new SqlConnection(connStr))
            {
                connection.Open();
                string query = "SELECT * FROM Application WHERE name = @name";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", application);
                SqlDataReader registos = cmd.ExecuteReader();
                int rowCount = 0;
                while (registos.Read())
                {
                    app.id = (int)registos["Id"];
                    rowCount++;
                }

                registos.Close();
                if (rowCount == 0) {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Não existe nenhuma aplicação com o nome " + application);
                    return response;
                }
            }

            var conts = new List<Container>();

            var headers = HttpContext.Current.Request.Headers;

            string somiodDiscover = headers.Get("somiod-discover"); // Meter somiod-discover nos headers no postman com value application

            if (somiodDiscover == "container")
            {

                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Container WHERE parent = @parent";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@parent", app.id);
                    SqlDataReader registos = cmd.ExecuteReader();
                    while (registos.Read())
                    {
                        Container cont = new Container();
                        cont.name = (string)registos["name"];
                        conts.Add(cont);
                    }
                }
            }

            // Cria um xml, dá-lhe declaração e cria os nodes
            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("response");
            doc.AppendChild(root);
            XmlElement appxml = doc.CreateElement("application");
            root.AppendChild(appxml);
            XmlElement nameapp = doc.CreateElement("name");
            nameapp.InnerText = application;
            appxml.AppendChild(nameapp);

            foreach (Container cont in conts)
            {
                XmlElement container = doc.CreateElement("container");
                XmlElement namecont = doc.CreateElement("name");
                namecont.InnerText = cont.name;
                container.AppendChild(namecont);
                appxml.AppendChild(container);
            }

            string xmlContent = doc.OuterXml;
            response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

            return response;
        }
    }
}