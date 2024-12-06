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

namespace App_B
{
    public partial class App_B : Form
    {
        MqttClient mqttClient;
        string[] topics = { "light_bulb" };

        public App_B()
        {
            InitializeComponent();
        }

        private void App_B_Load(object sender, EventArgs e)
        {
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

        private void App_B_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (mqttClient != null && mqttClient.IsConnected)
            {
                mqttClient.Disconnect();
            }
        }

        private void buttonON_Click(object sender, EventArgs e)
        {
            mqttClient.Publish(topics[0], Encoding.UTF8.GetBytes("on"));
        }

        private void buttonOff_Click(object sender, EventArgs e)
        {
            mqttClient.Publish(topics[0], Encoding.UTF8.GetBytes("off"));
        }
    }
}
