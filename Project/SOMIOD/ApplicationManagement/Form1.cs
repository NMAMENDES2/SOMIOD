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
using System.IO; // Havia conflitos com outra keyword de .net ent tem de ser assim



namespace ApplicationManagement
{
    public partial class Form1 : Form
    {

        string url = @"http://localhost:57806/api/somiod/";
        RestClient cliente = null;
        public Form1()
        {
            InitializeComponent();
            cliente = new RestClient(url);
        }

        private void getApplications_Click(object sender, EventArgs e)
        {

            richTextBoxListApplications.Clear();
            try
            {
                var request = new RestRequest("/", Method.Get);
                request.AddHeader("somiod-locate", "application");
                request.AddHeader("Content-Type", "application/xml");
                request.RequestFormat = DataFormat.Xml;

                var response = cliente.Execute(request);
                /*
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    richTextBoxListApplications.AppendText($"Error: {response.StatusCode} - {response.StatusDescription}\n");
                    return;
                }
                */
                var serializer = new XmlSerializer(typeof(App));
                using (var reader = new StringReader(response.Content))
                {
                    var data = (App)serializer.Deserialize(reader);
                    richTextBoxListApplications.AppendText(data.name + "\n");
                }
            }

            catch (Exception ex)
            {
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
    }
}
