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
    [RoutePrefix("api/somiod/notification")]
    public class NotificationController : ApiController
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
                var serializer = new XmlSerializer(typeof(Notification));
                using (StringReader reader = new StringReader(content))
                {
                    Notification notification = (Notification)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "INSERT INTO Notification (name, parent, event, endpoint, enabled) VALUES (@name, @parent, @event, @endpoint, @enabled)";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", notification.name);
                        cmd.Parameters.AddWithValue("@parent", notification.parent);
                        cmd.Parameters.AddWithValue("@event", notification.@event);
                        cmd.Parameters.AddWithValue("@endpoint", notification.endpoint);
                        cmd.Parameters.AddWithValue("@enabled", notification.enabled);
                        cmd.ExecuteNonQuery();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.Created, "Notification created successfully.");
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
            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT id, name, parent, event, endpoint, enabled FROM Notification";
                    SqlCommand cmd = new SqlCommand(query, connection);

                    SqlDataReader reader = cmd.ExecuteReader();
                    List<Notification> notifications = new List<Notification>();

                    while (reader.Read())
                    {
                        Notification notification = new Notification
                        {
                            id = (int)reader["id"],
                            name = reader["name"].ToString(),
                            parent = (int)reader["parent"],
                            @event = (int)reader["event"],
                            endpoint = reader["endpoint"].ToString(),
                            enabled = (bool)reader["enabled"]
                        };

                        notifications.Add(notification);
                    }

                    if (notifications.Count == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NoContent, "No notifications found.");
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, notifications);
                }
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
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "SELECT id, name, parent, event, endpoint, enabled FROM Notification WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        Notification notification = new Notification
                        {
                            id = (int)reader["id"],
                            name = reader["name"].ToString(),
                            parent = (int)reader["parent"],
                            @event = (int)reader["event"],
                            endpoint = reader["endpoint"].ToString(),
                            enabled = (bool)reader["enabled"]
                        };

                        return Request.CreateResponse(HttpStatusCode.OK, notification);
                    }
                    else
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Notification not found.");
                    }
                }
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [Route("update/{id}")]
        [HttpPut]
        public HttpResponseMessage Update(int id, HttpRequestMessage entity)
        {
            var content = entity.Content.ReadAsStringAsync().Result;

            try
            {
                var serializer = new XmlSerializer(typeof(Notification));
                using (StringReader reader = new StringReader(content))
                {
                    Notification notification = (Notification)serializer.Deserialize(reader);

                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string containerQuery = "SELECT COUNT(1) FROM Container WHERE Id = @parent";
                        SqlCommand containerCmd = new SqlCommand(containerQuery, connection);
                        containerCmd.Parameters.AddWithValue("@parent", notification.parent);

                        int containerExists = (int)containerCmd.ExecuteScalar();
                        if (containerExists == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.BadRequest, "Parent container does not exist.");
                        }

                        string query = "UPDATE Notification SET name = @name, parent = @parent, event = @event, " +
                                       "endpoint = @endpoint, enabled = @enabled WHERE id = @id";
                        SqlCommand cmd = new SqlCommand(query, connection);
                        cmd.Parameters.AddWithValue("@name", notification.name);
                        cmd.Parameters.AddWithValue("@parent", notification.parent);
                        cmd.Parameters.AddWithValue("@event", notification.@event);
                        cmd.Parameters.AddWithValue("@endpoint", notification.endpoint);
                        cmd.Parameters.AddWithValue("@enabled", notification.enabled);
                        cmd.Parameters.AddWithValue("@id", id);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Notification not found.");
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Notification updated successfully.");
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
                    string query = "DELETE FROM Notification WHERE id = @id";
                    SqlCommand cmd = new SqlCommand(query, connection);
                    cmd.Parameters.AddWithValue("@id", id);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected == 0)
                    {
                        return Request.CreateResponse(HttpStatusCode.NotFound, "Notification not found.");
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Notification deleted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        #endregion

    }
}