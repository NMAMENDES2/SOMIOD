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
        public HttpResponseMessage GetAllContainersOrRecords(string application) // Não dá com HTTPActionResult tem de ser assim
        {

            HttpResponseMessage response;
            Application app = new Application();
            var conts = new List<Container>();
            var recs = new List<Record>();
            var apps = new List<Application>();

            var headers = HttpContext.Current.Request.Headers;

            string somiodDiscover = headers.Get("somiod-locate"); // Meter somiod-discover nos headers no postman com value application

            XmlDocument doc = new XmlDocument();
            XmlDeclaration dec = doc.CreateXmlDeclaration("1.0", null, null);
            doc.AppendChild(dec);
            XmlElement root = doc.CreateElement("response");
            doc.AppendChild(root);

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
                    app.name = (string)registos["name"];
                    app.creation_datetime = (DateTime)registos["creation_datetime"];
                    apps.Add(app);
                    rowCount++;
                }

                registos.Close();
                if (rowCount == 0)
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Não existe nenhuma aplicação com o nome " + application);
                    return response;
                }
            }

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

                foreach (Container cont in conts)
                {
                    XmlElement container = doc.CreateElement("container");
                    XmlElement namecont = doc.CreateElement("name");
                    namecont.InnerText = cont.name;
                    container.AppendChild(namecont);
                    root.AppendChild(container);
                }
            }

            if (somiodDiscover == "record")
            {
                using (SqlConnection connection = new SqlConnection(connStr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Record WHERE parent = @parent";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@parent", app.id);
                    SqlDataReader registos = cmd.ExecuteReader();
                    while (registos.Read())
                    {
                        Record rec = new Record();
                        rec.name = (string)registos["name"];
                        recs.Add(rec);
                    }
                }

                foreach (Record rec in recs)
                {
                    XmlElement record = doc.CreateElement("record");
                    XmlElement nameRecord = doc.CreateElement("name");
                    nameRecord.InnerText = rec.name;
                    record.AppendChild(nameRecord);
                    root.AppendChild(record);
                }
            }

            if (somiodDiscover == null)
            {
                foreach (Application appaux in apps)
                {
                    XmlElement id = doc.CreateElement("id");
                    id.InnerText = app.id.ToString();
                    XmlElement name = doc.CreateElement("name");
                    name.InnerText = app.name;
                    XmlElement creation_datetime = doc.CreateElement("creation_date");
                    creation_datetime.InnerText = app.creation_datetime.ToString("o");
                    root.AppendChild(id);
                    root.AppendChild(name);
                    root.AppendChild(creation_datetime);  // tem de haver forma para isto n tar tão hard coded sque

                }
            }

            string xmlContent = doc.OuterXml;
            response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

            return response;
        }
    }
}