using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace Animal_Detection
{
    public partial class Form2 : Form
    {
        string fpath = @"...\hta.txt";
        string fpathres = @"...\res.txt";
        public Form2()
        {
            InitializeComponent();
            button1.Enabled = false;
            int count = System.IO.File.ReadAllLines(fpath).Length;
            string path = System.IO.File.ReadLines(fpath).First().Trim().Split(':')[1];

            pictureBox1.Image = Image.FromFile(path);
            progressBar1.Maximum = count;
            progressBar1.Value = 0;
            progressBar1.Step = 1;

            
            do
            {
                progressBar1.PerformStep();
                System.Threading.Thread.Sleep(5000);

            } while (count != 0);

 
            FileInfo fileInf1 = new FileInfo(fpath);
            while (!fileInf1.Exists)
            {
                System.Threading.Thread.Sleep(1000);
            }
            button1.Enabled = true;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
