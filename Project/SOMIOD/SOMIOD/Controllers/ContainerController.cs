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
using uPLibrary.Networking.M2Mqtt;
using SOMIOD.Utils;
using System.Reflection;
using System.Xml.Linq;

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
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();

                string query = @"SELECT COUNT(1)
                FROM Application AS app
                INNER JOIN Container AS cont ON cont.parent = app.Id
                WHERE app.name = @name AND cont.name = @containerName";

                using (SqlCommand cmd = new SqlCommand(query, conn)) {
                    cmd.Parameters.AddWithValue("@name", application);
                    cmd.Parameters.AddWithValue("@containerName", container);

                    int result = (int)cmd.ExecuteScalar();

                    return result > 0;
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

        private bool isEndpointValid(string endpoint) {
            string pattern = @"^(?:https?://|mqtt:/)?([\w.-]+)$";
            Regex regex = new Regex(pattern);

            Match match = regex.Match(endpoint);

            if (match.Success) {
                string cleanEndpoint = match.Groups[1].Value;

                try
                {
                    Dns.GetHostEntry(cleanEndpoint);
                    return true;
                }
                catch {
                    return false;
                }
            }

            return false;
        }
        private string getUniqueName()
        {
            return Guid.NewGuid().ToString();
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
            if (!(DBTransactions.nameExists(application, "Application")))
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
                return response;
            }

            if (!DBTransactions.nameExists(container, "Container"))
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Container does not exist");
                return response;
            }

            if (!DBTransactions.doesContainerBelongToApplication(application, container))
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, $"No container named {container} on application {application}");
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

        // Controller put container
        [Route("{application}/{container}")]
        [HttpPut]
        public HttpResponseMessage UpdateContainer(string application, string container)
        {
            HttpResponseMessage response;

            // Dados do pedido e validação
            byte[] bytes = RequestData.getRequestContenta(Request);

            if (bytes == null || bytes.Length == 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Couldn't process any data");
            }


            // Validar nome da aplicação
            if (!DBTransactions.nameExists(application, "Application"))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
            }

            // Validar nome do container
            if(!DBTransactions.nameExists(container, "Container"))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Container does not exist");
            }

            // validar se o container pertence à aplicação
            if (!DBTransactions.doesContainerBelongToApplication(application, container))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, $"No container named {container} on application {application}");
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

                // Validar root element
                if (root == null || root.Name != "request")
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid root element. Expecting <request>");
                }

                // Validar name
                if (nameNode != null && !string.IsNullOrWhiteSpace(nameNode.InnerText))
                {
                    name = nameNode.InnerText;
                }
                else { 
                    name = getUniqueName();
                }

                if(!resNode.InnerText.Equals("container", StringComparison.OrdinalIgnoreCase))
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid res_type");
                }

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
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Container name already exists");
                }
                return Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
            }

            catch (XmlException Ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Ex.Message);
            }

            catch (Exception Ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Ex.Message);
            }
        }


        // Feito acho eu

        [Route("{application}/{container}")]
        [HttpDelete]
        public HttpResponseMessage Delete(string application, string container)
        {
            HttpResponseMessage response;
            byte[] bytes;

            if (!DBTransactions.nameExists(application, "Application"))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound, "Application does not exist");
            }

            if (!DBTransactions.doesContainerBelongToApplication(application, container))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, $"No container named {container} on application {application}");
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

                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Ex.Message);
            }
        }

        [HttpPost]
        [Route("{application}/{container}")]
        public HttpResponseMessage CreateRecordOrNotificationOnContainer(string application, string container)
        {
            HttpResponseMessage response;
            byte[] bytes = RequestData.getRequestContenta(Request);

            if (bytes == null || bytes.Length == 0)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Couldn't process any data");
            }

            if (!DBTransactions.nameExists(application, "Application"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Application does not exist");
            }

            if (!DBTransactions.nameExists(container, "Container"))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Container does not exist");
            }

            if (!DBTransactions.doesContainerBelongToApplication(application, container))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest, "No container named " + container + " on that application");
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
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid root element. Expecting <request>");
                }

                if (nameNode != null && !string.IsNullOrWhiteSpace(nameNode.InnerText))
                {
                    name = nameNode.InnerText;
                }
                else
                {
                    name = getUniqueName();
                }


                if (resNode == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting res_type");
                }


                if (resNode.InnerText == "record")
                {
                    string[] topics = { container };
                    List<string> endpoints = new List<string>();

                    XmlNode contentNode = doc.SelectSingleNode("/request/content");
                    if (contentNode == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting content");
                    }

                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connstr))
                        {
                            conn.Open();
                            string query = "INSERT INTO Record (name, parent, content) VALUES (@name, @parent, @content)";
                            string querynotif = "SELECT * FROM Notification";
                            using (SqlCommand cmdNotif = new SqlCommand(querynotif, conn))
                            {
                                SqlDataReader reader = cmdNotif.ExecuteReader();
                                while (reader.Read())
                                {
                                    Notification notification = new Notification();
                                    notification.@event = (int)reader["event"];
                                    if (notification.@event == 1) {
                                        string endpoint = (string)reader["endpoint"];
                                        if (isEndpointValid(endpoint)) { 
                                           endpoints.Add(endpoint);
                                        }
                                    }
                                }
                                reader.Close();
                            }

                            foreach (string endpoint in endpoints) {
                                MqttClient mqttClient;
                                try
                                {
                                    mqttClient = new MqttClient(endpoint);
                                    mqttClient.Connect(Guid.NewGuid().ToString());
                                    mqttClient.Publish(topics[0], Encoding.UTF8.GetBytes(doc.OuterXml));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine(ex.Message);
                                }
                            }
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
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Record already exists");
                        }
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                    }
                }

                else if (resNode.InnerText == "notification")
                {
                    XmlNode endPointNode = doc.SelectSingleNode("/request/endpoint");
                    XmlNode eventNode = doc.SelectSingleNode("/request/event");
                    XmlNode enabledNode = doc.SelectSingleNode("/request/enabled");
                    var enabledNodeValue = "";
                    var endpoint = endPointNode.InnerText;

                    if (endPointNode == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting endpoint");
                    }

                    if (eventNode == null)
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting event");
                    }

                    if (enabledNode == null)
                    {
                        enabledNodeValue = "true";
                    }
                    else
                    {
                        enabledNodeValue = enabledNode.InnerText;
                    }

                    if ((eventNode.InnerText != "1" && eventNode.InnerText != "2"))
                    {
                        return Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting values 1 or 2 for the event");
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
                                cmd.Parameters.AddWithValue("@event", Convert.ToInt32(eventNode.InnerText));
                                cmd.Parameters.AddWithValue("@enabled", enabledNodeValue);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    return Request.CreateResponse(HttpStatusCode.OK, "Notification Created!");
                                }
                                else
                                {
                                    return Request.CreateResponse(HttpStatusCode.InternalServerError, "Error creating the notification");
                                }
                            }
                        }
                    }
                    catch (SqlException Ex)
                    {
                        if (Ex.Number == 2627)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Notification already exists");
                        }
                        return Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                    }


                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid res_type");
                }
            }
            catch (XmlException ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }
    }


}
