﻿using System;
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

            //createOpertion();

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

            Application application = new Application()
            {
                name = "Lighting",
            };

            Container container = new Container()
            {
                name = "light_bulb",
                parent = 10,
            };

            var appRequest = new RestRequest("", Method.Post);
            appRequest.RequestFormat = DataFormat.Xml;
            appRequest.AddXmlBody(application);
            var responseApp = client.Execute(appRequest);

            var contRequest = new RestRequest("/create", Method.Post);
            contRequest.RequestFormat = DataFormat.Xml;
            contRequest.AddXmlBody(container);
            var responseCont = client.Execute(contRequest);

            if (responseApp.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Failed to create application: {responseApp.StatusDescription}");
            }

            if (responseCont.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"Failed to create container: {responseCont.StatusDescription}");
            }
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
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }

        public class Application
        {
            [XmlElement("name")]
            public string name { get; set; }
        }

        public class Container
        {
            [XmlElement("name")]
            public string name { get; set; }
            [XmlElement("parent")]
            public int parent { get; set; }
        }
    }
}