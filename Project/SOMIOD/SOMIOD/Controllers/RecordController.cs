using SOMIOD.Models;
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

namespace SOMIOD.Controllers
{
    public class RecordController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;

        [HttpGet]
        [Route("{application}")]
        public HttpResponseMessage GetAllRecords(string application) // Não dá com HTTPActionResult tem de ser assim
        {

            HttpResponseMessage response;
            Application app = new Application();
            // Fetch db
            using (SqlConnection connection = new SqlConnection(connstr))
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

                if (rowCount == 0) {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Não existe nenhuma aplicação com nome " + application);
                    return response;
                }
            }

            var records = new List<Record>();

            var headers = HttpContext.Current.Request.Headers;

            string somiodDiscover = headers.Get("somiod-discover"); // Meter somiod-discover nos headers no postman com value application

            if (somiodDiscover == "record")
            {

                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Record WHERE parent = @parent";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@parent", app.id);
                    SqlDataReader registos = cmd.ExecuteReader();
                    while (registos.Read())
                    {
                        Record record = new Record();
                        record.name = (string)registos["name"];
                        records.Add(record);
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

            foreach (Record rec in records)
            {
                XmlElement record = doc.CreateElement("record");
                XmlElement nameRec = doc.CreateElement("name");
                nameRec.InnerText = rec.name;
                record.AppendChild(nameRec);
                appxml.AppendChild(record);
            }

            string xmlContent = doc.OuterXml;
            response = Request.CreateResponse(HttpStatusCode.OK);
            response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");

            return response;
        }

    }
}
