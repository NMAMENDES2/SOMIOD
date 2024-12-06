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
    [RoutePrefix("api/somiod")]
    public class NotificationAndRecordController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;

        #region CRUD's

        private int getParentID(string name)
        {
            Container container = null;
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string query = "SELECT * FROM Container WHERE name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            container = new Container();
                            container.id = (int)reader["Id"];
                        }
                    }

                }
            }
            return container.id;
        }


        private bool doesContainerBelongToApplication(string application, string container)
        {
            Application app = null;
            Container cont = null;

            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string queryApp = @"SELECT * FROM Application where name = @name";
                using (SqlCommand cmdApp = new SqlCommand(queryApp, conn))
                {
                    cmdApp.Parameters.AddWithValue("@name", application);
                    using (SqlDataReader reader = cmdApp.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app = new Application
                            {
                                id = (int)reader["Id"]
                            };
                        }
                    }
                }

                string queryContainer = @"SELECT 1 FROM Container WHERE parent = @appID";

                using (SqlCommand cmdContainer = new SqlCommand(queryContainer, conn))
                {
                    cmdContainer.Parameters.AddWithValue("@appID", app.id);
                    using (SqlDataReader readerContainer = cmdContainer.ExecuteReader())
                    {
                        return readerContainer.HasRows;
                    }
                }


            }
        }
        private bool doesRecordBelongToContainer(string container, string record)
        {
            Container cont = null;
            Record rec = null;

            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string queryContainer = @"SELECT * FROM Container where name = @name";
                using (SqlCommand cmdApp = new SqlCommand(queryContainer, conn))
                {
                    cmdApp.Parameters.AddWithValue("@name", container);
                    using (SqlDataReader reader = cmdApp.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cont = new Container
                            {
                                id = (int)reader["Id"]
                            };
                        }
                    }
                }

                string queryRecord = @"SELECT 1 FROM Record WHERE parent = @appID";

                using (SqlCommand cmdContainer = new SqlCommand(queryRecord, conn))
                {
                    cmdContainer.Parameters.AddWithValue("@appID", cont.id);
                    using (SqlDataReader readerContainer = cmdContainer.ExecuteReader())
                    {
                        return readerContainer.HasRows;
                    }
                }


            }
        }

        private bool doesNotificationBelongToContainer(string container, string notification)
        {
            Container cont = null;
            Notification notif = null;

            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string queryContainer = @"SELECT * FROM Container where name = @name";
                using (SqlCommand cmdApp = new SqlCommand(queryContainer, conn))
                {
                    cmdApp.Parameters.AddWithValue("@name", container);
                    using (SqlDataReader reader = cmdApp.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            cont = new Container
                            {
                                id = (int)reader["Id"]
                            };
                        }
                    }
                }

                string queryNotification = @"SELECT 1 FROM Notification WHERE parent = @appID";

                using (SqlCommand cmdContainer = new SqlCommand(queryNotification, conn))
                {
                    cmdContainer.Parameters.AddWithValue("@appID", cont.id);
                    using (SqlDataReader readerContainer = cmdContainer.ExecuteReader())
                    {
                        return readerContainer.HasRows;
                    }
                }


            }
        }

        private bool doesApplicationExist(string name)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string query = "SELECT * FROM Application WHERE name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows;

                    }
                }

            }
        }
        private string getUniqueName(string name)
        {
            if (doesApplicationExist(name))
            {
                return Guid.NewGuid().ToString();
            }
            else
            {
                return name;
            }
        }
        private bool doesContainerExist(string name)
        {
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string query = "SELECT * FROM Container WHERE name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        return reader.HasRows;

                    }
                } }
        }



        // ENDPOINTS DE TESTE

        [Route("{application}/{container}/record/{record}")]
        [HttpGet]
        public HttpResponseMessage GetRecord(string application, string container, string record)
        {
            Record rec = null;
            HttpResponseMessage response; 
            var responseXml = new StringWriter();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // remove a declaração <?xml ... ?>
                Indent = true
            };

            var isApplication = doesApplicationExist(application);
            if (!isApplication)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            var isContainer = doesContainerExist(application);
            if (!isContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Container does not exist");
                return response;
            }

            var isBelongContainer = doesContainerBelongToApplication(application, container);
            if (!isBelongContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }

            var isBelongRecord = doesRecordBelongToContainer(container, record);
            if (!isBelongRecord)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No record named " + record + " on that container");
                return response;

            }


            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT id, name, parent, content FROM Record WHERE name = @name";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", record);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Notification not found");
                        return response;
                    }
                    rec = new Record
                        {
                            id = (int)reader["id"],
                            name = reader["name"].ToString(),
                            parent = (int)reader["parent"],
                            content = (string)reader["content"],
                        };

                }
                    {
                        using (var writer = XmlWriter.Create(responseXml, settings))
                        {
                            writer.WriteStartElement("Record"); // personaliza o nó de raiz writer.WriteStartElement("Application"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                            writer.WriteElementString("id", rec.id.ToString());
                            writer.WriteElementString("name", rec.name);
                            writer.WriteElementString("creation_datetime", rec.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                            writer.WriteElementString("content", rec.content);
                            writer.WriteEndElement(); // fecha o nó <Container>
                        }

                        string xmlContent = responseXml.ToString();

                        response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                        response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                        return response;
                    }
                }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        [Route("{application}/{container}/notification/{notification}")]
        [HttpGet]
        public HttpResponseMessage GetNotification(string application, string container, string notification)
        {
            Notification notif = null;
            HttpResponseMessage response;
            var responseXml = new StringWriter();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // remove a declaração <?xml ... ?>
                Indent = true
            };

            var isApplication = doesApplicationExist(application);
            if (!isApplication)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            var isContainer = doesContainerExist(container);
            if (!isContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Container does not exist");
                return response;
            }

            var isBelongContainer = doesContainerBelongToApplication(application, container);
            if (!isBelongContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }

            var isBelongRecord = doesNotificationBelongToContainer(container, notification);
            if (!isBelongRecord)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No notification named " + notification + " on that container");
                return response;

            }


            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT id, name, parent, event, endpoint, enabled FROM Notification WHERE name = @name";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", notification);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (!reader.Read())
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Notification not found");
                        return response;
                    }
                        notif = new Notification
                        {
                            id = (int)reader["id"],
                            name = reader["name"].ToString(),
                            parent = (int)reader["parent"],
                            @event = (int)reader["event"],
                            endpoint = (string)reader["endpoint"],
                            enabled = (bool)reader["enabled"],

                        };

                }
                {
                    using (var writer = XmlWriter.Create(responseXml, settings))
                    {
                        writer.WriteStartElement("Notification"); // personaliza o nó de raiz writer.WriteStartElement("Application"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                        writer.WriteElementString("ID", notif.id.ToString());
                        writer.WriteElementString("name", notif.name);
                        writer.WriteElementString("creation_datetime", notif.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                        writer.WriteElementString("parent", notif.parent.ToString());
                        writer.WriteElementString("endpoint", notif.endpoint);
                        writer.WriteElementString("event", notif.@event.ToString());
                        writer.WriteElementString("enabled", notif.enabled.ToString());

                        writer.WriteEndElement(); // fecha o nó <Container>
                    }

                    string xmlContent = responseXml.ToString();

                    response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                    response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                    return response;
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("{application}/{container}/record/{record}")]
        [HttpDelete]
        public HttpResponseMessage DeleteRecord(string application, string container, string record)
        {
            HttpResponseMessage response;

            var isApplication = doesApplicationExist(application);
            if (!isApplication)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            var isContainer = doesContainerExist(container);
            if (!isContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Container does not exist");
                return response;
            }

            var isBelongContainer = doesContainerBelongToApplication(application, container);
            if (!isBelongContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }

            var isBelongRecord = doesRecordBelongToContainer(container, record);
            if (!isBelongRecord)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No record named " + record + " on that container");
                return response;

            }
            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "DELETE FROM Record WHERE name = @name";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", record);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, "Record not found");
                        return response;
                    }
                    response = Request.CreateResponse(HttpStatusCode.OK, "Record deleted");
                    return response;
                }
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }

        [Route("{application}/{container}/notification/{record}")]
        [HttpDelete]
        public HttpResponseMessage DeleteNotification(string application, string container, string record)
        {
            HttpResponseMessage response;

            var isApplication = doesApplicationExist(application);
            if (!isApplication)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            var isContainer = doesContainerExist(container);
            if (!isContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Container does not exist");
                return response;
            }

            var isBelongContainer = doesContainerBelongToApplication(application, container);
            if (!isBelongContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }

            var isBelongNotification = doesNotificationBelongToContainer(container, record);
            if (!isBelongNotification)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No notification named " + record + " on that container");
                return response;

            }
            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "DELETE FROM Notification WHERE name = @name";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", record);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        response = Request.CreateResponse(HttpStatusCode.NotFound, "Notification not found");
                        return response;
                    }
                }

                response = Request.CreateResponse(HttpStatusCode.OK, "Notification deleted");
                return response;
            }
            catch (Exception ex)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }





        #endregion

    }
}