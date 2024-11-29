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
using System.Data.SqlTypes;
using System.IO;
using System.Xml.Serialization;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ApplicationController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;

        [Route("create")]
        [HttpPost]
        public HttpResponseMessage Create(HttpRequestMessage entity)
        {
            var content = Request.Content.ReadAsStringAsync().Result;

            try
            {
                var serializer = new XmlSerializer(typeof(Application));

                // Usando StringReader para ler a string XML
                using (StringReader reader = new StringReader(content))
                {
                    // Desserializa o XML para o objeto Application
                    Application application = (Application)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "INSERT INTO Application (name) VALUES (@name)";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", application.name);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.Created, "Application created successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("getAll")]
        [HttpGet]
        // Terceiro endpoints dado no enunciado localhost/api/somiod
        public HttpResponseMessage GetAll() // Não dá com HTTPActionResult tem de ser assim
        {
            var apps = new List<Application>();

            try
            {
                var headers = HttpContext.Current.Request.Headers;
                string somiodDiscover = headers.Get("somiod-discover"); // Meter somiod-discover nos headers no postman com value application

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
                            Application app = new Application
                            {
                                id = (int)registos["id"],
                                name = (string)registos["name"]
                            };
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
                    XmlElement applic = doc.CreateElement("application");
                    XmlElement id = doc.CreateElement("id");
                    id.InnerText = app.id.ToString();
                    XmlElement name = doc.CreateElement("name");
                    name.InnerText = app.name;
                    applic.AppendChild(id);
                    applic.AppendChild(name);
                    root.AppendChild(applic);
                }

                string xmlContent = doc.OuterXml;
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("get/{id}")]
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                Application app = null;
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Application WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader registos = cmd.ExecuteReader();
                    if (registos.Read())
                    {
                        app = new Application
                        {
                            name = (string)registos["name"]
                        };
                    }
                }

                if (app == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Application not found.");
                }

                return Request.CreateResponse(HttpStatusCode.OK, app);
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("update/{id}")]
        [HttpPut]
        public HttpResponseMessage Update(int id, HttpRequestMessage request)
        {
            try
            {
                // Lê o conteúdo da requisição (XML)
                var content = request.Content.ReadAsStringAsync().Result;

                // Usando o XmlSerializer para desserializar o XML para um objeto Application
                var serializer = new XmlSerializer(typeof(Application));

                using (StringReader reader = new StringReader(content))
                {
                    // Desserializa o XML para o objeto Application
                    Application application = (Application)serializer.Deserialize(reader);

                    // Verifica se o ID do objeto Application corresponde ao ID da URL
                    if (application.id != id)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "ID mismatch.");
                    }

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();

                        // Atualiza os dados na tabela Application
                        string query = "UPDATE Application SET name = @name WHERE id = @id";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", application.name);
                        cmd.Parameters.AddWithValue("@id", application.id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Verifica se a atualização foi bem-sucedida
                        if (rowsAffected == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Application not found.");
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Application updated successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("delete/{id}")]
        [HttpDelete]
        public HttpResponseMessage Delete(int id)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "DELETE FROM Application WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Application not found.");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Application deleted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }
}