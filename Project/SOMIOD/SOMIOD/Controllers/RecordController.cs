using SOMIOD.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;
using System.Xml;
using System.Xml.Serialization;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod/record")]
    public class RecordController : ApiController
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
                var serializer = new XmlSerializer(typeof(Record));
                using (StringReader reader = new StringReader(content))
                {
                    Record record = (Record)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "INSERT INTO Record (name, content, parent) VALUES (@name, @content, @parent)";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", record.name);
                        cmd.Parameters.AddWithValue("@content", record.content);
                        cmd.Parameters.AddWithValue("@parent", record.parent);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.Created, "Record created successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("getAll")]
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            var records = new List<Record>();

            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT Id, name, content, creation_datetime, parent FROM Record";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    SqlDataReader registos = cmd.ExecuteReader();

                    while (registos.Read())
                    {
                        Record record = new Record
                        {
                            id = (int)registos["Id"],
                            name = (string)registos["name"],
                            content = (string)registos["content"],
                            creation_datetime = (DateTime)registos["creation_datetime"],
                            parent = (int)registos["parent"]
                        };
                        records.Add(record);
                    }
                }

                var responseXml = new StringWriter();
                var settings = new XmlWriterSettings
                {
                    OmitXmlDeclaration = true,
                    Indent = true
                };

                using (var writer = XmlWriter.Create(responseXml, settings))
                {
                    writer.WriteStartElement("Records");
                    foreach (var record in records)
                    {
                        writer.WriteStartElement("Record");
                        writer.WriteElementString("id", record.id.ToString());
                        writer.WriteElementString("name", record.name);
                        writer.WriteElementString("content", record.content);
                        writer.WriteElementString("creation_datetime", record.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                        writer.WriteElementString("parent", record.parent.ToString());
                        writer.WriteEndElement();
                    }
                    writer.WriteEndElement();
                }

                string xmlContent = responseXml.ToString();

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
                Record record = null;

                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT * FROM Record WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        record = new Record
                        {
                            id = (int)reader["id"],
                            name = (string)reader["name"],
                            content = (string)reader["content"],
                            creation_datetime = (DateTime)reader["creation_datetime"],
                            parent = (int)reader["parent"]
                        };
                    }
                }

                if (record == null)
                {
                    return Request.CreateResponse(HttpStatusCode.NotFound, "Record not found.");
                }

                var serializer = new XmlSerializer(typeof(Record));

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

                        serializer.Serialize(xmlWriter, record, namespaces);
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

                var serializer = new XmlSerializer(typeof(Record));

                using (StringReader reader = new StringReader(content))
                {
                    Record updatedRecord = (Record)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();

                        string query = "UPDATE Record SET name = @name, content = @content, parent = @parent WHERE id = @id";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", updatedRecord.name);
                        cmd.Parameters.AddWithValue("@content", updatedRecord.content);
                        cmd.Parameters.AddWithValue("@parent", updatedRecord.parent);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Record not found.");
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Record updated successfully.");
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
                    string query = "DELETE FROM Record WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Record not found.");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Record deleted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        #endregion
    }
}
