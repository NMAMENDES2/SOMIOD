using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Http;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using SOMIOD.Models;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ContainerController : ApiController
    {
        string connstr = Properties.Settings.Default.ConString;
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
                }

            }
        }

        // Feito acho eu

        [Route("{application}/{container}")]
        [HttpGet]
        public HttpResponseMessage GetContainer(string application, string container)
        {

            HttpResponseMessage response;

            var responseXml = new StringWriter();
            var settings = new XmlWriterSettings { 
                OmitXmlDeclaration = true,
                Indent = true
            };

            Container cont = null;
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

            var isBelong = doesContainerBelongToApplication(application, container);
            if (!isBelong)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }


            using (SqlConnection connection = new SqlConnection(connstr))
            {
                connection.Open();
                string query = "SELECT * FROM Container WHERE name = @name";
                SqlCommand cmd = new SqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@name", container);
                SqlDataReader reader = cmd.ExecuteReader();

                if (reader.Read())
                {
                    cont = new Container
                    {
                        id = (int)reader["id"],
                        name = (string)reader["name"],
                        creation_datetime = (DateTime)reader["creation_datetime"],
                        parent = (int)reader["parent"]
                    };
                }
            }

            if (cont == null)
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Container not found.");
            }

            var headers = HttpContext.Current.Request.Headers;
            string somiodLocate = headers.Get("somiod-locate");
            if (somiodLocate == "record")
            {
                var records = new List<Record>();
                try
                {
                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM Record WHERE parent = @parent";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@parent", cont.id);
                        SqlDataReader registos = cmd.ExecuteReader();
                        while (registos.Read())
                        {
                            Record record = new Record
                            {
                                id = (int)registos["id"],
                                name = (string)registos["name"],
                                creation_datetime = registos["creation_datetime"] == DBNull.Value ? DateTime.MinValue : (DateTime)registos["creation_datetime"],
                                parent = (int)registos["parent"],
                                content = (string)registos["content"],

                            };
                            records.Add(record);
                        }
                    }
                    using (var writer = XmlWriter.Create(responseXml, settings))
                    {
                        writer.WriteStartElement("Response"); // personaliza o nó de raiz
                        foreach (var record in records)
                        {
                            writer.WriteStartElement("Record"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                            writer.WriteElementString("ID", record.id.ToString());
                            writer.WriteElementString("name", record.name);
                            writer.WriteElementString("creation_datetime", record.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                            writer.WriteElementString("parent", record.parent.ToString());
                            writer.WriteElementString("content", record.content.ToString());
                            writer.WriteEndElement(); // fecha o nó <Container>
                        }
                        writer.WriteEndElement(); // fecha o nó de raiz <Containers>

                    }

                    string xmlContent = responseXml.ToString();

                    response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                    response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                    return response;

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            else if (somiodLocate == "notification")
            {

                var notifs = new List<Notification>();
                try
                {
                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM Notification WHERE parent = @parent";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@parent", cont.id);
                        SqlDataReader registos = cmd.ExecuteReader();
                        while (registos.Read())
                        {
                            Notification notification = new Notification
                           {
                                id = (int)registos["id"],
                                name = (string)registos["name"],
                                creation_datetime = registos["creation_datetime"] == DBNull.Value ? DateTime.MinValue : (DateTime)registos["creation_datetime"],
                                parent = (int)registos["parent"],
                                @event = (int)registos["event"],
                                endpoint = (string)registos["endpoint"],
                                enabled = (bool)registos["enabled"],
                            };
                            notifs.Add(notification);
                        }
                    }
                    using (var writer = XmlWriter.Create(responseXml, settings))
                    {
                        writer.WriteStartElement("Response"); // personaliza o nó de raiz
                        foreach (var notif in notifs)
                        {
                            writer.WriteStartElement("Notification"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                            writer.WriteElementString("ID", notif.id.ToString());
                            writer.WriteElementString("name", notif.name);
                            writer.WriteElementString("creation_datetime", notif.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                            writer.WriteElementString("parent", notif.parent.ToString());
                            writer.WriteElementString("endpoint", notif.endpoint);
                            writer.WriteElementString("event", notif.@event.ToString());
                            writer.WriteElementString("enabled", notif.enabled.ToString());
                            writer.WriteEndElement(); // fecha o nó <Container>
                        }
                        writer.WriteEndElement(); // fecha o nó de raiz <Containers>

                    }

                    string xmlContent = responseXml.ToString();

                    response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                    response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                    return response;

                }
                catch (Exception ex)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
            try
            {
                using (var writer = XmlWriter.Create(responseXml, settings))
                {
                    writer.WriteStartElement("Container"); // personaliza o nó de raiz writer.WriteStartElement("Application"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                    writer.WriteElementString("id", cont.id.ToString());
                    writer.WriteElementString("name", cont.name);
                    writer.WriteElementString("creation_datetime", cont.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                    writer.WriteEndElement(); // fecha o nó <Container>
                }

                string xmlContent = responseXml.ToString();

                response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                return response;

            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }

        // Feito acho eu

        [Route("{application}/{container}")]
        [HttpPut]
        public HttpResponseMessage Update(string application, string container)
        {
            HttpResponseMessage response;
            byte[] bytes;

            var isValid = doesApplicationExist(application);
            if (!isValid)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            var isBelong = doesContainerBelongToApplication(application, container);
            if (!isBelong)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }

            using (Stream stream = Request.Content.ReadAsStreamAsync().Result)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }

            if (bytes == null || bytes.Length == 0)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Couldn't process any data");
                return response;
            }

            string xmlContent = Encoding.UTF8.GetString(bytes);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlContent);
                XmlNode root = doc.DocumentElement;
                XmlNode nameNode = doc.SelectSingleNode("/request/name");
                XmlNode resNode = doc.SelectSingleNode("/request/res_type");
                string name;

                if (root == null || root.Name != "request")
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid root element. Expecting <request>");
                    return response;

                }
                if (nameNode != null && !string.IsNullOrWhiteSpace(nameNode.InnerText))
                {
                    name = nameNode.InnerText;
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "No name to update");
                    return response;
                }


                if (resNode.InnerText == "container")
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connstr))
                        {
                            conn.Open();
                            string query = "UPDATE Container SET name = @name WHERE name = @namePrev";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@namePrev", container);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    response = Request.CreateResponse(HttpStatusCode.OK, "Container Updated!");
                                    return response;
                                }
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Container does not exist");
                                    return response;
                                }
                            }
                        }
                    }
                    catch (SqlException Ex)
                    {
                        if (Ex.Number == 2627)
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Container name already exists");
                            return response;
                        }
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                        return response;
                    }
                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid res_type");
                    return response;
                }
            }
            catch (XmlException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }


        // Feito acho eu

        [Route("{application}/{container}")]
        [HttpDelete]
        public HttpResponseMessage Delete(string application, string container)
        {
            HttpResponseMessage response;
            byte[] bytes;

            var isValid = doesApplicationExist(application);
            if (!isValid)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            var isBelong = doesContainerBelongToApplication(application, container);
            if (!isBelong)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
                return response;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();

                    string query = "DELETE FROM Container WHERE name = @name";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@name", container);

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Container not found.");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Container deleted successfully.");
            }
            catch (SqlException Ex)
            {

                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Ex.Message);
                return response;
            }
        }

        [HttpPost]
        [Route("{application}/{container}")]
        public HttpResponseMessage CreateRecordOrNotificationOnContainer(string application, string container)
        {
            HttpResponseMessage response;
            byte[] bytes;

            var isApplication = doesApplicationExist(application);
            if (!isApplication)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Application does not exist");
                return response;
            }

            var isContainer = doesContainerExist(container);
            if (!isContainer)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Container does not exist");
                return response;
            }

            var isBelong = doesContainerBelongToApplication(application, container);
            if (!isBelong)
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Container does not belong to that application");
                return response;
            }


            using (Stream stream = Request.Content.ReadAsStreamAsync().Result)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }
            }

            if (bytes == null || bytes.Length == 0)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Couldn't process any data");
                return response;
            }

            string xmlContent = Encoding.UTF8.GetString(bytes);
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(xmlContent);
                XmlNode root = doc.DocumentElement;
                XmlNode nameNode = doc.SelectSingleNode("/request/name");
                XmlNode resNode = doc.SelectSingleNode("/request/res_type");
                string name;

                if (root == null || root.Name != "request")
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid root element. Expecting <request>");
                    return response;
                }

                if (nameNode != null && !string.IsNullOrWhiteSpace(nameNode.InnerText))
                {
                    name = nameNode.InnerText;
                }
                else
                {
                    name = getUniqueName(Guid.NewGuid().ToString());
                }


                if (resNode == null)
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting res_type");
                    return response;
                }


                if (resNode.InnerText == "record")
                {
                    XmlNode contentNode = doc.SelectSingleNode("/request/content");
                    if (contentNode == null)
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting content");
                        return response;
                    }

                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connstr))
                        {
                            conn.Open();
                            string query = "INSERT INTO Record (name, parent, content) VALUES (@name, @parent, @content)";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@parent", getParentID(container));
                                cmd.Parameters.AddWithValue("@content", contentNode.InnerText);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    response = Request.CreateResponse(HttpStatusCode.OK, "Record Created!");
                                    return response;
                                }
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Error creating the record");
                                    return response;
                                }
                            }
                        }
                    }
                    catch (SqlException Ex)
                    {
                        if (Ex.Number == 2627)
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Record already exists");
                            return response;
                        }
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                        return response;
                    }
                }

                else if (resNode.InnerText == "notification")
                {
                    XmlNode endPointNode = doc.SelectSingleNode("/request/endpoint");
                    XmlNode eventNode = doc.SelectSingleNode("/request/event");
                    XmlNode enabledNode = doc.SelectSingleNode("/request/enabled");
                    var endpoint = endPointNode.InnerText;

                    if (endPointNode == null)
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting endpoint");
                        return response;
                    }

                    if (!(endpoint.StartsWith("mqtt://") || endpoint.StartsWith("http://"))) {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Endpoint must begin with mqtt:// or htpp://");
                        return response;
                    }

                    var ip = endpoint.Substring(7);
                    string pattern = @"^((([0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5])\.){3}([0-1]?[0-9]{1,2}|2[0-4][0-9]|25[0-5]))$";
                    bool isValid = Regex.IsMatch(ip, pattern);
                    if (!isValid) {
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Invalid IP Address");
                        return response;
                    }

                    if (eventNode == null)
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting event");
                        return response;
                    }

                    if (enabledNode == null)
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting enabled");
                        return response;
                    }

                    if ((eventNode.InnerText != "1" && eventNode.InnerText != "2"))
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting values 1 or 2 for the event");
                        return response;
                    }


                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connstr))
                        {
                            conn.Open();
                            string query = "INSERT INTO Notification (name, parent, endpoint, event, enabled) VALUES (@name, @parent, @endpoint, @event, @enabled)";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@parent", getParentID(container));
                                cmd.Parameters.AddWithValue("@endpoint", endPointNode.InnerText);
                                cmd.Parameters.AddWithValue("@event", eventNode.InnerText);
                                cmd.Parameters.AddWithValue("@enabled", enabledNode.InnerText);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    response = Request.CreateResponse(HttpStatusCode.OK, "Notification Created!");
                                    return response;
                                }
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Error creating the notification");
                                    return response;
                                }
                            }
                        }
                    }
                    catch (SqlException Ex)
                    {
                        if (Ex.Number == 2627)
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Notification already exists");
                            return response;
                        }
                        response = Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                        return response;
                    }


                }
                else
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid res_type");
                    return response;
                }
            }
            catch (XmlException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }
        }
    }


}
