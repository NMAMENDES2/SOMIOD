using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;
using SOMIOD.Models;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod/containers")]
    public class ContainerController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;

        #region CRUD's

        [Route("create")]
        [HttpPost]
        public HttpResponseMessage Create(HttpRequestMessage entity)
        {
            var content = entity.Content.ReadAsStringAsync().Result;

            try
            {
                var serializer = new XmlSerializer(typeof(Container));
                using (StringReader reader = new StringReader(content))
                {
                    Container container = (Container)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "INSERT INTO Container (name, parent) VALUES (@name, @parent)";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", container.name);
                        cmd.Parameters.AddWithValue("@parent", container.parent);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.Created, "Container created successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("/")]
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            var containers = new List<Container>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT Id, name, creation_datetime, parent FROM Container";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader registos = cmd.ExecuteReader();

                    while (registos.Read())
                    {
                        Container container = new Container
                        {
                            id = (int)registos["Id"],
                            name = (string)registos["name"],
                            creation_datetime = (DateTime)registos["creation_datetime"],
                            parent = (int)registos["parent"]
                        };
                        containers.Add(container);
                    }
                }

                var responseXml = new StringWriter();
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true, // remove a declaração <?xml ... ?>
                    Indent = true               
                };

                using (var writer = XmlWriter.Create(responseXml, settings))
                {
                    writer.WriteStartElement("Containers"); // personaliza o nó de raiz
                    foreach (var container in containers)
                    {
                        writer.WriteStartElement("Container"); // cada item será representado como um nó <Container>
                        writer.WriteElementString("id", container.id.ToString());
                        writer.WriteElementString("name", container.name);
                        writer.WriteElementString("creation_datetime", container.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                        writer.WriteElementString("parent", container.parent.ToString());
                        writer.WriteEndElement(); // fecha o nó <Container>
                    }
                    writer.WriteEndElement(); // fecha o nó de raiz <Containers>
                }

                // recupera o conteúdo final do XML gerado
                string xmlContent = responseXml.ToString();

                // retorna a resposta em XML
                var response = Request.CreateResponse(HttpStatusCode.OK);
                response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                return response;
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("get/{id}")] // devia ser {id}?
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            try
            {
                Container container = null;

                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Container WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        container = new Container
                        {
                            id = (int)reader["id"],
                            name = (string)reader["name"],
                            creation_datetime = (DateTime)reader["creation_datetime"],
                            parent = (int)reader["parent"]
                        };
                    }
                }

                if (container == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Container not found.");
                }

                var serializer = new XmlSerializer(typeof(Container));

                using (var stringWriter = new StringWriter())
                {
                    var xmlWriterSettings = new XmlWriterSettings
                    {
                        OmitXmlDeclaration = true, 
                        Indent = true 
                    };

                    using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                    {
                        var namespaces = new XmlSerializerNamespaces();
                        namespaces.Add("", "");

                        serializer.Serialize(xmlWriter, container, namespaces);
                    }

                    var response = Request.CreateResponse(HttpStatusCode.OK);
                    response.Content = new StringContent(stringWriter.ToString(), Encoding.UTF8, "application/xml");
                    return response;
                }
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
                var content = request.Content.ReadAsStringAsync().Result;

                var serializer = new XmlSerializer(typeof(Container));

                using (StringReader reader = new StringReader(content))
                {
                    Container updatedContainer = (Container)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();

                        string query = "UPDATE Container SET name = @name, parent = @parent WHERE id = @id";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", updatedContainer.name);
                        cmd.Parameters.AddWithValue("@parent", updatedContainer.parent);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Container not found.");
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Container updated successfully.");
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

                    string query = "DELETE FROM Container WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Container not found.");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Container deleted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        #endregion 


    }
}
