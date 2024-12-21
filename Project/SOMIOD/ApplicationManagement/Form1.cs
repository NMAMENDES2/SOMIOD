using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using RestSharp;
using App = SOMIOD.Models.Application;
using System.Xml.Serialization;
using System.IO;
using System.Xml; // Havia conflitos com outra keyword de .net ent tem de ser assim



namespace ApplicationManagement
{
    public partial class Form1 : Form
    {

        string url = @"http://localhost:57806/api/somiod/";
        RestClient cliente = null;
        public Form1()
        {
            InitializeComponent();
            
        }

        

        private void btnNameApp_Click(object sender, EventArgs e)
        {
            string nameApp = textBoxNameApp.Text;

            //cliente = new RestClient(url);

            string rawXml = $@"<request>
                    <name>{nameApp}</name> 
                    <res_type>application</res_type>
                  </request>";

            var appRequest = new RestRequest("", Method.Post);
            appRequest.AddHeader("Content-Type", "application/xml");
            appRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseApp = cliente.Execute(appRequest);

            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Failed to create application: {responseApp.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Created Successfully.");
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            cliente = new RestClient(url);
                //METER NO COMBOBOX AS APPS!
            var appRequest = new RestRequest("", Method.Get); // Ajuste o endpoint conforme necessário
            appRequest.AddHeader("somiod-locate", "application");
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                // XML
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseApp.Content);
                //Containers
                comboBoxAppsContainers.Items.Clear();
                comboBoxAppsConts.Items.Clear();
                comboBoxDeleteAppsConts.Items.Clear();
                comboBoxDetailCont.Items.Clear();
                //Notifications
                comboBoxAppNotifCreate.Items.Clear();
                comboBoxDeleteNotifApp.Items.Clear();
                comboBoxDetailsApplicationNotif.Items.Clear();
                //Records
                comboBoxAppCreateRecord.Items.Clear();
                comboBoxDeleteRecordApplicationName.Items.Clear();
                comboBoxDetailsRecordApplicationName.Items.Clear();
                // XPATH
                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        //Containers
                        comboBoxAppsContainers.Items.Add(nameNode.InnerText);
                        comboBoxAppsConts.Items.Add(nameNode.InnerText);
                        comboBoxDeleteAppsConts.Items.Add(nameNode.InnerText);
                        comboBoxDetailCont.Items.Add(nameNode.InnerText);
                        //Notifications
                        comboBoxAppNotifCreate.Items.Add(nameNode.InnerText);
                        comboBoxDeleteNotifApp.Items.Add(nameNode.InnerText);
                        comboBoxDetailsApplicationNotif.Items.Add(nameNode.InnerText);
                        //Records
                        comboBoxAppCreateRecord.Items.Add(nameNode.InnerText);
                        comboBoxDeleteRecordApplicationName.Items.Add(nameNode.InnerText);
                        comboBoxDetailsRecordApplicationName.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No application names found in the response.");
                }
            }

 
    }

        private void btnUpdateApp_Click(object sender, EventArgs e)
        {
            string newNameApp = textBoxUpdNewAppName.Text;
            string oldNameApp = textBoxUpdOldNameApp.Text;

            var appRequest = new RestRequest($"{oldNameApp}", Method.Get);

            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the old app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                string rawXml = $@"<request>
                    <name>{newNameApp}</name> 
                    <res_type>application</res_type>
                  </request>";

                appRequest = new RestRequest($"{oldNameApp}", Method.Put);
                appRequest.AddHeader("Content-Type", "application/xml");
                appRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

                responseApp = cliente.Execute(appRequest);

                if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show($"Failed to update application: {responseApp.StatusDescription}");
                }
                else
                {
                    MessageBox.Show("Updated Successfully.");
                }
            }
        }

        private void btnDelApp_Click(object sender, EventArgs e)
        {
            string delAppName = textBoxDELApp.Text;

            var appRequest = new RestRequest($"{delAppName}", Method.Delete);
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Deleted Successfully.");
            }
        }

        private void btnGetDetailsApp_Click(object sender, EventArgs e)
        {
            string nameApp = textBoxGetDetailsApp.Text;

            var appRequest = new RestRequest($"{nameApp}", Method.Get);
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseApp.Content);

                // XML XPATH
                var idNode = xmlDoc.SelectSingleNode("//id");
                var nameNode = xmlDoc.SelectSingleNode("//name");
                var creationDateNode = xmlDoc.SelectSingleNode("//creation_datetime");

                lbldetailsid.Text = "ID: ";
                lblnamedetails.Text = "Name: ";
                lbldetailscreationtime.Text = "Creation Date: ";

                // Set the labels with the extracted values
                if (idNode != null && nameNode != null && creationDateNode != null)
                {
                    lbldetailsid.Text += idNode.InnerText;
                    lblnamedetails.Text += nameNode.InnerText;
                    lbldetailscreationtime.Text += creationDateNode.InnerText;
                }
                else
                {
                    MessageBox.Show("Error parsing the response content.");
                }
            }
        }

        private void btnContCreate_Click(object sender, EventArgs e)
        {
            string contName = textBoxNameContainerCreate.Text;
            string appName = comboBoxAppsContainers.SelectedItem.ToString();

            string rawXml = $@"<request>
                    <name>{contName}</name> 
                    <res_type>container</res_type>
                  </request>";

            var appRequest = new RestRequest($"{appName}/", Method.Post);
            appRequest.AddHeader("Content-Type", "container/xml");
            appRequest.AddParameter("container/xml", rawXml, ParameterType.RequestBody);

            var responseApp = cliente.Execute(appRequest);

            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                MessageBox.Show("done");
            }
        }

        private void btnUpdCont_Click(object sender, EventArgs e)
        {
            string contNameOld = textBoxOldContName.Text;
            string contNameNew = textBoxContUpd.Text;
            string appName = comboBoxAppsConts.SelectedItem.ToString();

            var appRequest = new RestRequest($"{appName}/{contNameOld}", Method.Get);

            var responseApp = cliente.Execute(appRequest);
            //MessageBox.Show(responseApp.Content);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the old app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                string rawXml = $@"<request>
                    <name>{contNameNew}</name> 
                    <res_type>container</res_type>
                  </request>";

                appRequest = new RestRequest($"{appName}/{contNameOld}", Method.Put);
                appRequest.AddHeader("Content-Type", "container/xml");
                appRequest.AddParameter("container/xml", rawXml, ParameterType.RequestBody);

                responseApp = cliente.Execute(appRequest);

                if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show($"Failed to update container: {responseApp.StatusDescription}");
                }
                else
                {
                    MessageBox.Show("Updated Successfully.");
                }
            }
        }

        private void btnDelCont_Click(object sender, EventArgs e)
        {
            string contName = textBoxContainerDel.Text;
            string appName = comboBoxAppsConts.SelectedItem.ToString();


            var appRequest = new RestRequest($"{appName}/{contName}", Method.Delete);

            var responseApp = cliente.Execute(appRequest);
            //MessageBox.Show(responseApp.Content);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the old app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Deleted Successfully.");
            }
        }

        private void btnContDetails_Click(object sender, EventArgs e)
        {
            string nameCont = txtBoxDetailsCont.Text;
            string appName = comboBoxDetailCont.SelectedItem.ToString();

            labelDetContID.Text = "ID: ";
            labelDetNameCont.Text = "Name: ";
            labelDetContCreationDate.Text = "Creation Date: ";
            labelDetailsContainerParent.Text = "Application: ";

            var appRequest = new RestRequest($"{appName}/{nameCont}", Method.Get);
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseApp.Content);

                // XML XPATH
                var idNode = xmlDoc.SelectSingleNode("//id");
                var nameNode = xmlDoc.SelectSingleNode("//name");
                var creationDateNode = xmlDoc.SelectSingleNode("//creation_datetime");
                //var nameAppNode = xmlDoc.SelectSingleNode("//parent");
                labelDetContID.Text = "ID: ";
                labelDetNameCont.Text = "Name: ";
                labelDetContCreationDate.Text = "Creation Date: ";
                labelDetailsContainerParent.Text = "Application: ";
                MessageBox.Show(responseApp.Content);
                // Set the labels with the extracted values
                if (idNode != null && nameNode != null && creationDateNode != null) //&& nameAppNode != null)
                {
                    labelDetContID.Text += idNode.InnerText;
                    labelDetNameCont.Text += nameNode.InnerText;
                    labelDetContCreationDate.Text += creationDateNode.InnerText;
                    labelDetailsContainerParent.Text += comboBoxDetailCont.SelectedItem.ToString();
                }
                else
                {
                    MessageBox.Show("Error parsing the response content.");
                }
            }
        }


        private void btnCreateNotif_Click(object sender, EventArgs e)
        {
            string notifName = textBoxNotifCreateName.Text;
            string contName = comboBoxNotifContCreate.SelectedItem.ToString();
            string appName = comboBoxAppNotifCreate.SelectedItem.ToString();
            string endpoint = textBoxEndpointCreate.Text;
            string evenT = "";

            if (checkBoxCreate.Checked)
            {
                evenT = "1";
            } else if(checkBoxDelete.Checked) {
                evenT = "2";
            } else
            {
                MessageBox.Show("ERROR! YOU HAVE TO CHECK ONE EVENT!");
                return;
            }


            string rawXml = $@"<request> 
                        <name>{notifName}</name>
                        <event>{evenT}</event>
                        <parent>{contName}</parent>
                        <endpoint>{endpoint}</endpoint>
                        <res_type>notification</res_type>
                      </request>";

            var onNotiRequest = new RestRequest($"/{appName}/{contName}", Method.Post);
            onNotiRequest.AddHeader("Content-Type", "application/xml");
            onNotiRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseOnNoti = cliente.Execute(onNotiRequest);

            if (responseOnNoti.StatusCode != HttpStatusCode.BadRequest && responseOnNoti.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseOnNoti.StatusDescription}");
            }
            else
            {
                MessageBox.Show("done");
            }   
        }

        private void updateComboBoxApplications()
        {
            var appRequest = new RestRequest("", Method.Get); // Ajuste o endpoint conforme necessário
            appRequest.AddHeader("somiod-locate", "application");
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                // XML
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseApp.Content);
                //Containers
                comboBoxAppsContainers.Items.Clear();
                comboBoxAppsConts.Items.Clear();
                comboBoxDeleteAppsConts.Items.Clear();
                comboBoxDetailCont.Items.Clear();
                //Notifications
                comboBoxAppNotifCreate.Items.Clear();
                comboBoxDeleteContNotif.Items.Clear();
                // XPATH
                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        //Containers
                        comboBoxAppsContainers.Items.Add(nameNode.InnerText);
                        comboBoxAppsConts.Items.Add(nameNode.InnerText);
                        comboBoxDeleteAppsConts.Items.Add(nameNode.InnerText);
                        comboBoxDetailCont.Items.Add(nameNode.InnerText);
                        //Notifications
                        comboBoxAppNotifCreate.Items.Add(nameNode.InnerText);
                        comboBoxDeleteContNotif.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No application names found in the response.");
                }
            }
        }
        
        
   
        

        public bool isUpdatingComboBoxContainers = false;

        
        private void comboBoxNotifContCreate_SelectedIndexChanged(object sender, EventArgs e)
        {

            if (comboBoxNotifContCreate.SelectedItem != null)
            {
                isUpdatingComboBoxContainers = true;
                updateComboBoxApplications();
                isUpdatingComboBoxContainers = false;
            }
            else
            {
                MessageBox.Show("Selecione um container.");
            }
        }




        //Notifications
        private bool isUpdatingComboBoxApp = false;
        private bool isUpdatingComboBoxAppDel = false;
        private bool isUpdatingComboBoxDetails = false;
        //Records
        private bool isUpdatingComboBoxAppRecCreate = false;
        private bool isUpdatingComboBoxAppRecDelete = false;
        private bool isUpdatingComboBoxAppRecDetails = false;

        private void comboBoxAppNotifCreate_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUpdatingComboBoxApp && comboBoxAppNotifCreate.SelectedItem != null)
            {
                isUpdatingComboBoxApp = true;
                uptadeComboBoxContainersCreate();
                isUpdatingComboBoxApp = false;
            }
        }

        private void comboBoxDeleteNotifApp_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUpdatingComboBoxAppDel && comboBoxDeleteNotifApp.SelectedItem != null)
            {
                isUpdatingComboBoxAppDel = true;
                uptadeComboBoxContainersDelete();
                isUpdatingComboBoxAppDel = false;
            }
            
            if (comboBoxDeleteContNotif.Items.Count > 0)
            {
                comboBoxDeleteContNotif.SelectedIndex = 0;
            }
        }

        private void uptadeComboBoxContainersCreate()
        {
            var contRequest = new RestRequest($"{comboBoxAppNotifCreate.SelectedItem.ToString()}/", Method.Get);
            contRequest.AddHeader("somiod-locate", "container");
            var responseCont = cliente.Execute(contRequest);

            if (responseCont.StatusCode == HttpStatusCode.OK)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseCont.Content);
                comboBoxNotifContCreate.Items.Clear();

                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        comboBoxNotifContCreate.Items.Add(nameNode.InnerText);
                    }

                    if (comboBoxNotifContCreate.Items.Count > 0)
                    {
                        comboBoxNotifContCreate.SelectedIndex = 0;
                    }
                }
                else
                {
                    MessageBox.Show("No container names found in the response.");
                }
            }
            else
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseCont.StatusDescription}");
            }
        }


        private void uptadeComboBoxContainersDelete()
        {
            var contRequest = new RestRequest($"{comboBoxDeleteNotifApp.SelectedItem.ToString()}/", Method.Get);
            contRequest.AddHeader("somiod-locate", "container");
            var responseCont = cliente.Execute(contRequest);

            if (responseCont.StatusCode == HttpStatusCode.OK)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseCont.Content);
                comboBoxDeleteContNotif.Items.Clear();

                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        comboBoxDeleteContNotif.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No container names found in the response.");
                }
            }
            else
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseCont.StatusDescription}");
            }
        }


        

        private void uptadeComboBoxContainersDetails()
        {
            var contRequest = new RestRequest($"{comboBoxDetailsApplicationNotif.SelectedItem.ToString()}/", Method.Get);
            contRequest.AddHeader("somiod-locate", "container");
            var responseCont = cliente.Execute(contRequest);

            if (responseCont.StatusCode == HttpStatusCode.OK)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseCont.Content);
                comboBoxDetailContNotif.Items.Clear();

                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        comboBoxDetailContNotif.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No container names found in the response.");
                }
            }
            else
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseCont.StatusDescription}");
            }
        }
        private void btnDeleteNotif_Click(object sender, EventArgs e)
        {
            string NotifName = textBoxDeleteNotifName.Text;
            string NotifCont = comboBoxDeleteContNotif.SelectedItem.ToString();
            string NotifApp = comboBoxDeleteNotifApp.SelectedItem.ToString();

            var appRequest = new RestRequest($"{NotifApp}/{NotifCont}/notification/{NotifName}", Method.Delete);
            var responseApp = cliente.Execute(appRequest);
            //MessageBox.Show(responseApp.Content);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the old app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Deleted Successfully.");
            }
        }

        private void btnGetDetailsNotif_Click(object sender, EventArgs e)
        {
            string nameNotif = textBoxDetailsNotifcationName.Text;
            string ContNotif = comboBoxDetailContNotif.SelectedItem.ToString();
            string AppNotif = comboBoxDetailsApplicationNotif.SelectedItem.ToString();

            labelDetailsIDNotif.Text = "ID: ";
            labelNameDetailsNotif.Text = "Name: ";
            labelDetailsCreationDateNotif.Text = "Creation Date: ";
            labelDetailsContainerNotif.Text = "Container: ";
            labelDetailsEventNotif.Text = "Event: ";
            labelDetailsEndpointNotif.Text = "Endpoint: ";

            var appRequest = new RestRequest($"{AppNotif}/{ContNotif}/notification/{nameNotif}", Method.Get);
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {

                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseApp.Content);

                // XML XPATH
                
                var idNode = xmlDoc.SelectSingleNode("//Notification/ID");
                var nameNode = xmlDoc.SelectSingleNode("//Notification/name");
                var creationDateNode = xmlDoc.SelectSingleNode("//Notification/creation_datetime");
                var eventNode = xmlDoc.SelectSingleNode("//Notification/event");
                var endpointNode = xmlDoc.SelectSingleNode("//Notification/endpoint");

                //var nameAppNode = xmlDoc.SelectSingleNode("//parent");
                labelDetailsIDNotif.Text = "ID: ";
                labelNameDetailsNotif.Text = "Name: ";
                labelDetailsCreationDateNotif.Text = "Creation Date: ";
                labelDetailsContainerNotif.Text = "Container: ";
                labelDetailsEventNotif.Text = "Event: ";
                labelDetailsEndpointNotif.Text = "Endpoint: ";
                //MessageBox.Show(responseApp.Content);
                // Set the labels with the extracted values
                if (idNode != null && nameNode != null && creationDateNode != null) //&& nameAppNode != null)
                {
                    labelDetailsIDNotif.Text += idNode.InnerText;
                    labelNameDetailsNotif.Text += nameNode.InnerText;
                    labelDetailsCreationDateNotif.Text += creationDateNode.InnerText;
                    labelDetailsContainerNotif.Text += ContNotif.ToString();
                    labelDetailsEventNotif.Text += eventNode.InnerText;
                    labelDetailsEndpointNotif.Text += endpointNode.InnerText;
                }
                else
                {
                    MessageBox.Show("Error parsing the response content.");
                }
            }
        }

        private void comboBoxDetailsApplicationNotif_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUpdatingComboBoxDetails && comboBoxDetailsApplicationNotif.SelectedItem != null)
            {
                isUpdatingComboBoxDetails = true;
                uptadeComboBoxContainersDetails();
                isUpdatingComboBoxDetails = false;
            }

            if (comboBoxDetailContNotif.Items.Count > 0)
            {
                comboBoxDetailContNotif.SelectedIndex = 0;
            }
        }
        
        private void btnCreateRec_Click(object sender, EventArgs e)
        {
            string recordName = textBoxRecordNameCreate.Text;
            string containerName = comboBoxContainerRecCreate.SelectedItem.ToString();
            string applicationName = comboBoxAppCreateRecord.SelectedItem.ToString();
            string content = textBoxContentRecCreate.Text;


            string rawXml = $@"<request> 
                        <name>{recordName}</name>
                        <parent>{containerName}</parent>
                        <content> {content} </content>
                        <res_type>record</res_type>
                      </request>";

            var recordRequest = new RestRequest($"/{applicationName}/{containerName}", Method.Post);
            recordRequest.AddHeader("Content-Type", "application/xml");
            recordRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseRecord = cliente.Execute(recordRequest);

            if (responseRecord.StatusCode != HttpStatusCode.BadRequest && responseRecord.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseRecord.StatusDescription}");
            }
            else
            {
                MessageBox.Show("done");
            }
        }


        private void uptadeComboBoxContainersRecordCreate()
        {
            var contRequest = new RestRequest($"{comboBoxAppCreateRecord.SelectedItem.ToString()}/", Method.Get);
            contRequest.AddHeader("somiod-locate", "container");
            var responseCont = cliente.Execute(contRequest);

            if (responseCont.StatusCode == HttpStatusCode.OK)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseCont.Content);
                comboBoxContainerRecCreate.Items.Clear();

                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        comboBoxContainerRecCreate.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No container names found in the response.");
                }
            }
            else
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseCont.StatusDescription}");
            }
        }

        private void comboBoxAppCreateRecord_SelectedIndexChanged(object sender, EventArgs e)
        
            {
                if (!isUpdatingComboBoxDetails && comboBoxAppCreateRecord.SelectedItem != null)
                {
                    isUpdatingComboBoxDetails = true;
                    uptadeComboBoxContainersRecordCreate();
                    isUpdatingComboBoxDetails = false;
                }

                if (comboBoxContainerRecCreate.Items.Count > 0)
                {
                    comboBoxContainerRecCreate.SelectedIndex = 0;
                }
            }

        private void btnDelRec_Click(object sender, EventArgs e)
        {
            string recordName = textBoxNameDeleteRecord.Text;
            string recordCont = comboBoxDeleteRecordContainerName.SelectedItem.ToString();
            string recordApp = comboBoxDeleteRecordApplicationName.SelectedItem.ToString();

            var appRequest = new RestRequest($"{recordApp}/{recordCont}/record/{recordName}", Method.Delete);
            var responseApp = cliente.Execute(appRequest);
            //MessageBox.Show(responseApp.Content);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the old app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Deleted Successfully.");
            }
        }

        private void comboBoxDeleteRecordApplicationName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUpdatingComboBoxAppRecDelete && comboBoxDeleteRecordApplicationName.SelectedItem != null)
            {
                isUpdatingComboBoxAppRecDelete = true;
                uptadeComboBoxContainersRecordDelete();
                isUpdatingComboBoxAppRecDelete = false;
            }

            if (comboBoxDeleteRecordContainerName.Items.Count > 0)
            {
                comboBoxDeleteRecordContainerName.SelectedIndex = 0;
            }
        }

        private void uptadeComboBoxContainersRecordDelete()
        {
            var contRequest = new RestRequest($"{comboBoxDeleteRecordApplicationName.SelectedItem.ToString()}/", Method.Get);
            contRequest.AddHeader("somiod-locate", "container");
            var responseCont = cliente.Execute(contRequest);

            if (responseCont.StatusCode == HttpStatusCode.OK)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseCont.Content);
                comboBoxDeleteRecordContainerName.Items.Clear();

                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        comboBoxDeleteRecordContainerName.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No container names found in the response.");
                }
            }
            else
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseCont.StatusDescription}");
            }
        }

        private void btnGetDetailsRec_Click(object sender, EventArgs e)
        {
            string nameRecord = textBoxDetailsRecordName.Text;
            string contRecord = comboBoxDetailsRecordContainerName.SelectedItem.ToString();
            string appRecord = comboBoxDetailsRecordApplicationName.SelectedItem.ToString();

            labelDetailsIDRec.Text = "ID: ";
            labelDetailsNameRec.Text = "Name: ";
            labelDetailsCreateDateRec.Text = "Creation Date: ";
            labelDetailsContentRec.Text = "Content: ";

            var appRequest = new RestRequest($"{appRecord}/{contRecord}/record/{nameRecord}", Method.Get);
            var responseApp = cliente.Execute(appRequest);
            if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
            }
            else
            {

                var xmlDoc = new XmlDocument();
                if(responseApp.StatusCode != HttpStatusCode.OK)
                {
                    MessageBox.Show($"ERROR! the app name might be wrong!\n {responseApp.StatusDescription}");
                    return;
                }
                xmlDoc.LoadXml(responseApp.Content);


                // XML XPATH
                //MessageBox.Show(responseApp.Content);
                var idNode = xmlDoc.SelectSingleNode("//Record/id");
                var nameNode = xmlDoc.SelectSingleNode("//Record/name");
                var creationDateNode = xmlDoc.SelectSingleNode("//Record/creation_datetime");
                var contentNode = xmlDoc.SelectSingleNode("//Record/content");

                //var nameAppNode = xmlDoc.SelectSingleNode("//parent");
                labelDetailsIDRec.Text = "ID: ";
                labelDetailsNameRec.Text = "Name: ";
                labelDetailsCreateDateRec.Text = "Creation Date: ";
                labelDetailsContentRec.Text = "Content: ";
                //MessageBox.Show(responseApp.Content);
                // Set the labels with the extracted values
                if (idNode != null && nameNode != null && creationDateNode != null) //&& nameAppNode != null)
                {
                    labelDetailsIDRec.Text += idNode.InnerText;
                    labelDetailsNameRec.Text += nameNode.InnerText;
                    labelDetailsCreateDateRec.Text += creationDateNode.InnerText;
                    labelDetailsContentRec.Text += contentNode.InnerText;
                }
                else
                {
                    MessageBox.Show("Error parsing the response content.");
                }
            }
        }

        private void labelDetailsIDRec_Click(object sender, EventArgs e)
        {

        }

        private void comboBoxDetailsRecordApplicationName_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isUpdatingComboBoxAppRecDetails && comboBoxDetailsRecordApplicationName.SelectedItem != null)
            {
                isUpdatingComboBoxAppRecDetails = true;
                uptadeComboBoxContainersRecordDetails();
                isUpdatingComboBoxAppRecDetails = false;
            }

            if (comboBoxDetailsRecordContainerName.Items.Count > 0)
            {
                comboBoxDetailsRecordContainerName.SelectedIndex = 0;
            }
        }

        private void uptadeComboBoxContainersRecordDetails()
        {
            var contRequest = new RestRequest($"{comboBoxDetailsRecordApplicationName.SelectedItem.ToString()}/", Method.Get);
            contRequest.AddHeader("somiod-locate", "container");
            var responseCont = cliente.Execute(contRequest);

            if (responseCont.StatusCode == HttpStatusCode.OK)
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseCont.Content);
                comboBoxDetailsRecordContainerName.Items.Clear();

                var nameNodes = xmlDoc.SelectNodes("//name");
                if (nameNodes != null)
                {
                    foreach (XmlNode nameNode in nameNodes)
                    {
                        comboBoxDetailsRecordContainerName.Items.Add(nameNode.InnerText);
                    }
                }
                else
                {
                    MessageBox.Show("No container names found in the response.");
                }
            }
            else
            {
                MessageBox.Show($"ERROR! Something went wrong!\n {responseCont.StatusDescription}");
            }
        }
    }
}
