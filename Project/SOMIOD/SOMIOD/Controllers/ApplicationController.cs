﻿using System;
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
using System.Web.Caching;
using System.Runtime.InteropServices.WindowsRuntime;
using SOMIOD.Utils;
using System.Web.UI.WebControls;

namespace SOMIOD.Controllers
{
    [RoutePrefix("api/somiod")]
    public class ApplicationController : ApiController
    {

        string connstr = Properties.Settings.Default.ConString;

        /// <summary>
        /// verifica se o nome da aplicação já existe na BD
        /// </summary>
        private bool doesNameExistDB(string name)
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
                        return reader.HasRows;  // se houver registros, retorna true
                    }
                }

            }
        }

        /// <summary>
        /// retorna o ID da aplicação com base no nome.
        /// </summary>
        private int getParentID(string name)
        {
            Application app = null;
            using (SqlConnection conn = new SqlConnection(connstr))
            {
                conn.Open();
                string query = "SELECT * FROM Application WHERE name = @name";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@name", name);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            app = new Application();
                            app.id = (int)reader["Id"];
                        }
                    }
                }
            }
            // retorna -1 no caso de não encontrar nenhuma aplicação 
            if (app == null) {
                return -1;
            }
            return app.id;
        }

        /// <summary>
        /// gera um nome único se o nome fornecido já existir na BD
        /// </summary>
        private string getUniqueName(string name)
        {
            if (doesNameExistDB(name))
            {
                return Guid.NewGuid().ToString();
            }
            else
            {
                return name;
            }
        }

        // -----------------------------------------------------------------------------

        #region CRUD's
        /// <summary>
        /// cria uma nova aplicação com base nos dados XML enviados no body do request
        /// </summary>
        [Route("")]
        [HttpPost]
        public HttpResponseMessage CreateApplication()
       {
            HttpResponseMessage response;
            byte[] bytes;

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

                // define o nome da aplicação
                if (nameNode != null && !string.IsNullOrWhiteSpace(nameNode.InnerText))
                {
                    name = nameNode.InnerText;
                }
                else
                {
                    name = getUniqueName(Guid.NewGuid().ToString());
                }

                // validação do res_type
                if (resNode.InnerText != "application")
                {
                    response = Request.CreateResponse(HttpStatusCode.BadRequest, "Invalid res_type");
                    return response;
                }

                // insere o nome da aplicação na BD
                try
                {
                    using (SqlConnection conn = new SqlConnection(connstr))
                    {
                        conn.Open();
                        string query = "INSERT INTO Application (name) VALUES (@name)";
                        using (SqlCommand cmd = new SqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@name", name);
                            int rows = cmd.ExecuteNonQuery();
                            if (rows > 0)
                            {
                                response = Request.CreateResponse(HttpStatusCode.OK, "Application Created!");
                                return response;
                            }
                            else
                            {
                                response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Error creating the application");
                                return response;
                            }
                        }
                    }
                }
                catch (SqlException Ex)
                {
                    if (Ex.Number == 2627)
                    {
                        response = Request.CreateResponse(HttpStatusCode.BadRequest, "Application already exists");
                        return response;
                    }
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                    return response;
                }

            }
            catch (XmlException ex)
            {
                response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, ex.Message);
                return response;
            }

        }

        /// <summary>
        /// retorna todas as aplicações em formato XML
        /// </summary>
        [Route("")]
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            var apps = new List<Application>();
            HttpResponseMessage response;
            var headers = HttpContext.Current.Request.Headers;
            string somiodLocate = headers.Get("somiod-locate");

            if (string.IsNullOrEmpty(somiodLocate))
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Missing header");
                return response;
            }

            var responseXml = new StringWriter();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // remove a declaração <?xml ... ?>
                Indent = true
            };

            // Fetch db
            if (somiodLocate == "application")
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM Application";
                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            using (SqlDataReader registos = cmd.ExecuteReader())
                            {
                                while (registos.Read())
                                {
                                    Application app = new Application
                                    {
                                        id = (int)registos["id"],
                                        name = (string)registos["name"],
                                        creation_datetime = registos["creation_datetime"] == DBNull.Value ? DateTime.MinValue : (DateTime)registos["creation_datetime"]
                                    };
                                    apps.Add(app);
                                }
                            }
                        }
                    }

                }
                catch (SqlException Ex)
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                    return response;
                }

                try
                {
                    // converte os dados em XML
                    using (var writer = XmlWriter.Create(responseXml, settings))
                    {
                        writer.WriteStartElement("Response"); // personaliza o nó de raiz
                        foreach (var app in apps)
                        {
                            writer.WriteStartElement("Application"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                            writer.WriteElementString("ID", app.id.ToString());
                            writer.WriteElementString("name", app.name);
                            writer.WriteElementString("creation_datetime", app.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                            writer.WriteEndElement(); // fecha o nó <Container>
                        }
                        writer.WriteEndElement(); // fecha o nó de raiz <Containers>

                    }
                    string xmlContent = responseXml.ToString();
                    response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                    response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                    return response;
                }
                catch (XmlException Ex)
                {
                    response = Request.CreateErrorResponse(HttpStatusCode.InternalServerError, Ex.Message);
                    return response;
                }
            }
            else
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Expecting value application");
                return response;
            }
        }

        [Route("{application}")]
        [HttpGet]
        public HttpResponseMessage GetApplication(string application)
        {
            var responseXml = new StringWriter();
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true, // remove a declaração <?xml ... ?>
                Indent = true // formata a saída XML para ser legível
            };

            HttpResponseMessage response;
            var headers = HttpContext.Current.Request.Headers;
            string somiodLocate = headers.Get("somiod-locate");


            Application app = null;
            // conexão para procurar a aplicação pelo nome
            using (SqlConnection connection = new SqlConnection(connstr))
            {
                connection.Open();
                string query = "SELECT * FROM Application WHERE name = @name";
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.Parameters.AddWithValue("@name", application);
                    using (SqlDataReader registos = cmd.ExecuteReader())
                    {
                        if (registos.Read())
                        {
                            app = new Application
                            {
                                id = (int)registos["id"],
                                name = (string)registos["name"]
                            };
                        }
                    }
                }
            }

            if (app == null)
            {
                response = Request.CreateResponse(HttpStatusCode.NotFound, "Application not found.");
                return response;
            }

            // verifica se é necessário retornar os containers associados
            if (somiodLocate == "container")
            {
                var containers = new List<Container>();
                try
                {
                    using (SqlConnection connection = new SqlConnection(connstr))
                    {
                        connection.Open();
                        string query = "SELECT * FROM Container WHERE parent = @parent";
                        using (SqlCommand cmd = new SqlCommand(query, connection))
                        {
                            cmd.Parameters.AddWithValue("@parent", app.id);
                            using (SqlDataReader registos = cmd.ExecuteReader())
                            {
                                while (registos.Read())
                                {
                                    // preenche a lista de containers
                                    Container container = new Container
                                    {
                                        id = (int)registos["id"],
                                        name = (string)registos["name"],
                                        creation_datetime = registos["creation_datetime"] == DBNull.Value ? DateTime.MinValue : (DateTime)registos["creation_datetime"],
                                        parent = (int)registos["parent"]
                                    };
                                    containers.Add(container);
                                }
                            }
                        }
                    }

                    // gera XML com os dados dos containers
                    using (var writer = XmlWriter.Create(responseXml, settings))
                    {
                        writer.WriteStartElement("Response"); // personaliza o nó de raiz
                        foreach (var container in containers)
                        {
                            writer.WriteStartElement("Container"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                            writer.WriteElementString("ID", container.id.ToString());
                            writer.WriteElementString("name", container.name);
                            writer.WriteElementString("creation_datetime", container.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
                            writer.WriteElementString("parent", container.parent.ToString());
                            writer.WriteEndElement(); // fecha o nó <Container>
                        }
                        writer.WriteEndElement(); // fecha o nó de raiz <Containers>

                    }

                    // retorna a resposta XML que foi formatada
                    string xmlContent = responseXml.ToString();

                    response = Request.CreateResponse(HttpStatusCode.OK, xmlContent);
                    response.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
                    return response;

                }
                catch (Exception Ex)
                {
                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, Ex.Message);
                    return response;
                }
            }
            try
            {
                using (var writer = XmlWriter.Create(responseXml, settings))
                {
                    writer.WriteStartElement("Application"); // personaliza o nó de raiz writer.WriteStartElement("Application"); // cada item será representado como um nó <Container> writer.WriteElementString("id", app.id.ToString());
                    writer.WriteElementString("id", app.id.ToString());
                    writer.WriteElementString("name", app.name);
                    writer.WriteElementString("creation_datetime", app.creation_datetime.ToString("yyyy-MM-ddTHH:mm:ss.fff"));
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

        /// <summary>
        /// método PUT para atualizar o nome de uma aplicacao
        /// </summary>

        [Route("{application}")]
        [HttpPut]
        public HttpResponseMessage UpdateApplication(string application) 
        {
            HttpResponseMessage response;
            byte[] bytes;
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


                if (resNode.InnerText == "application")
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connstr))
                        {
                            conn.Open();
                            string query = "UPDATE Application SET name = @name WHERE name = @namePrev";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@namePrev", application);
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    response = Request.CreateResponse(HttpStatusCode.OK, "Application Updated!");
                                    return response;
                                }
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Application does not exist");
                                    return response;
                                }
                            }
                        }
                    }
                    catch (SqlException Ex)
                    {
                        if (Ex.Number == 2627)
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Application name already exists");
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

       /// <summary>
       /// método DELETE para apagar uma aplicacao
       /// </summary>
        [Route("{application}")]
        [HttpDelete]
        public HttpResponseMessage Delete(string application)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connstr))
                {
                    connection.Open();
                    string query = "DELETE FROM Application WHERE name = @name";
                    using (SqlCommand cmd = new SqlCommand(query, connection))
                    {
                        cmd.Parameters.AddWithValue("@name", application);
                        int rowsAffected = cmd.ExecuteNonQuery();

                        if (rowsAffected == 0)
                        {
                            return Request.CreateResponse(HttpStatusCode.NotFound, "Application not found.");
                        }
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, "Application deleted successfully.");
            }
            catch (Exception ex)
            {
                return Request.CreateResponse(HttpStatusCode.InternalServerError, ex.Message);
            }
        }


        [HttpPost]
        [Route("{application}")]
        public HttpResponseMessage createContainerOnAppliction(string application)
        {

            HttpResponseMessage response;
            byte[] bytes;

            if (!DBTransactions.nameExists(application, "Application"))
            {
                response = Request.CreateResponse(HttpStatusCode.BadRequest, "Application does not exist");
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

                if (resNode.InnerText == "container")
                {
                    try
                    {
                        using (SqlConnection conn = new SqlConnection(connstr))
                        {
                            conn.Open();
                            string query = "INSERT INTO Container (name, parent) VALUES (@name, @parent)";
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@name", name);
                                cmd.Parameters.AddWithValue("@parent", getParentID(application));
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0)
                                {
                                    response = Request.CreateResponse(HttpStatusCode.OK, "Container Created!");
                                    return response;
                                }
                                else
                                {
                                    response = Request.CreateResponse(HttpStatusCode.InternalServerError, "Error creating the container");
                                    return response;
                                }
                            }
                        }
                    }
                    catch (SqlException Ex)
                    {
                        if (Ex.Number == 2627)
                        {
                            response = Request.CreateResponse(HttpStatusCode.BadRequest, "Container already exists");
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

        #endregion
    }
}