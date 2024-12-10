using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt.Messages;
using uPLibrary.Networking.M2Mqtt;
using RestSharp;
using System.Net;
using System.Security.Policy;

namespace App_B
{
    public partial class App_B : Form
    {
        MqttClient mqttClient;
        string[] topics = { "light_bulb" };

        string url = @"http://localhost:57806/api/somiod/";
        RestClient client = null;

        public App_B()
        {
            InitializeComponent();
        }

        private void App_B_Load(object sender, EventArgs e)
        {
            createOperation();

            try
            {
                mqttClient = new MqttClient("127.0.0.1");
                mqttClient.Connect(Guid.NewGuid().ToString());
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect to broker: {ex.Message}");
                return;
            }

            if (!mqttClient.IsConnected)
            {
                MessageBox.Show("MQTT client is not connected");
                return;
            }
        }

        private void createOperation()
        {
            client = new RestClient(url);

            string rawXml = @"<request> 
                                <name>Switch</name>
                                <res_type>application</res_type> 
                              </request>";

            var appRequest = new RestRequest("", Method.Post);
            appRequest.AddHeader("Content-Type", "application/xml");
            appRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseApp = client.Execute(appRequest);

            if (responseApp.StatusCode != HttpStatusCode.OK && responseApp.StatusCode != HttpStatusCode.BadRequest)
            {
                MessageBox.Show($"Failed to create application: {responseApp.StatusDescription}");
            }
        }

        private void App_B_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }

        private void buttonON_Click(object sender, EventArgs e)
        {
            string rawXml = @"<request>
                        <content>on</content>
                        <res_type>record</res_type> 
                      </request>";

            var recordRequest = new RestRequest("/Lighting/light_bulb", Method.Post);
            recordRequest.AddHeader("Content-Type", "application/xml");
            recordRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseRecord = client.Execute(recordRequest);

            /*if (responseRecord.StatusCode != HttpStatusCode.OK && responseRecord.StatusCode != HttpStatusCode.BadRequest)
            {
                MessageBox.Show($"Failed to create application: {responseRecord.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Record Created!");
            }*/

            //mqttClient.Publish(topics[0], Encoding.UTF8.GetBytes("on"));
        }


        private void buttonOff_Click(object sender, EventArgs e)
        {
            string rawXml = @"<request>
                        <content>off</content>
                        <res_type>record</res_type> 
                      </request>";

            var recordRequest = new RestRequest("/Lighting/light_bulb", Method.Post);
            recordRequest.AddHeader("Content-Type", "application/xml");
            recordRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseRecord = client.Execute(recordRequest);

            /*if (responseRecord.StatusCode != HttpStatusCode.OK && responseRecord.StatusCode != HttpStatusCode.BadRequest)
            {
                MessageBox.Show($"Failed to create record: {responseRecord.StatusDescription}");
            }
            else
            {
                MessageBox.Show("Record Created!");
            }*/

            //mqttClient.Publish(topics[0], Encoding.UTF8.GetBytes("off"));
        }
    }
}
