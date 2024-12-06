using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using uPLibrary.Networking.M2Mqtt;

namespace App_A
{
    public partial class App_A : Form
    {
        private bool isOn = false;
        MqttClient mqttClient;

        public App_A()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.Image = Properties.Resources.off;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (isOn)
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = Properties.Resources.off;
            }
            else
            {
                pictureBox1.Image.Dispose();
                pictureBox1.Image = Properties.Resources.on; 
            }

            isOn = !isOn;
        }
    }
}
