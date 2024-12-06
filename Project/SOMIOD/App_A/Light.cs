using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml.Serialization;
using RestSharp;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;

namespace App_A
{
    public partial class App_A : Form
    {
        MqttClient mqttClient;
        string[] topics = { "light_bulb" };
        byte[] qosLevels = { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE };

        string url = @"http://localhost:57806/api/somiod/";
        RestClient client = null;

        public App_A()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.off;

            createOpertion();

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

            mqttClient.MqttMsgPublishReceived += MqttClient_MqttMsgPublishReceived;
            mqttClient.Subscribe(topics, qosLevels);
        }

        private void createOpertion()
        {
            client = new RestClient(url);

            string rawXml = @"<request>
                                <name>Lighting</name> 
                                <res_type>application</res_type> 
                              </request>";

            var appRequest = new RestRequest("", Method.Post);
            appRequest.AddHeader("Content-Type", "application/xml");
            appRequest.AddParameter("application/xml", rawXml, ParameterType.RequestBody);

            var responseApp = client.Execute(appRequest);

            /*if (responseApp.StatusCode != HttpStatusCode.BadRequest && responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Failed to create application: {responseApp.StatusDescription}");
            }*/

            rawXml = @"<request> 
                        <name>light_bulb</name>
                        <res_type>container</res_type> 
                    </request>";

            var contRequest = new RestRequest("/Lighting", Method.Post);
            contRequest.AddHeader("Content-Type", "container/xml");
            contRequest.AddParameter("container/xml", rawXml, ParameterType.RequestBody);

            var responseCont = client.Execute(contRequest);

            /*if (responseCont.StatusCode != HttpStatusCode.BadRequest && responseCont.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Failed to create application: {responseCont.StatusDescription}");
            }*/
        }

        private void MqttClient_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string topic = e.Topic;
            string msg = Encoding.UTF8.GetString(e.Message);

            if (topic != topics[0])
                MessageBox.Show("WRONG TOPIC");

            this.Invoke((MethodInvoker)(() =>
            {
                if (msg.Equals("on", StringComparison.OrdinalIgnoreCase))
                {
                    pictureBox1.Image = Properties.Resources.on;
                }
                else if (msg.Equals("off", StringComparison.OrdinalIgnoreCase))
                {
                    pictureBox1.Image = Properties.Resources.off;
                }
                else
                {
                    MessageBox.Show("Invalid message");
                }
            }));
        }

        private void App_A_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                if (mqttClient != null && mqttClient.IsConnected)
                {
                    mqttClient.Disconnect();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error during MQTT disconnection: {ex.Message}");
            }
        }
    }
}