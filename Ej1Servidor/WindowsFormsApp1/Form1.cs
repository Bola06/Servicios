using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using System.Net.Sockets;


namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        IPAddress ip = IPAddress.Parse("127.0.0.1");
        int puerto = 31416;

        private async Task<string> EnvioYRecepcionAsync()
        {
            try
            {
                using (Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp))
                {
                    IPEndPoint ep = new IPEndPoint(ip, puerto);

                    await s.ConnectAsync(ep);

                    Encoding codificacion = Console.OutputEncoding;

                    using (NetworkStream ns = new NetworkStream(s))
                    using (StreamReader sr = new StreamReader(ns, codificacion))
                    using (StreamWriter sw = new StreamWriter(ns, codificacion))
                    {
                        sw.AutoFlush = true;

                        await sr.ReadLineAsync();

                        await sw.WriteLineAsync(textBox1.Text);

                        string msg = await sr.ReadLineAsync();
                        return msg;
                    }
                }   
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private async void button1_Click(object sender, EventArgs e)
        {
            textBox1.Text = "time";
            string msg = await EnvioYRecepcionAsync();
            label1.Text = msg;
            textBox1.Text = "";
        }

        private async void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "date";
            string msg = await EnvioYRecepcionAsync();
            label1.Text = msg;
            textBox1.Text = "";
        }

        private async void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "all";
            string msg = await EnvioYRecepcionAsync();
            label1.Text = msg;
            textBox1.Text = "";
        }

        private async void button4_Click(object sender, EventArgs e)
        {
            string contra = textBox1.Text.Trim();
            textBox1.Text = "close " + contra;
            string msg = await EnvioYRecepcionAsync();
            label1.Text = msg;
            textBox1.Text = "";

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }
        
    }
}
