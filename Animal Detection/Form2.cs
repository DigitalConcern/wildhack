using System;
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
        public string ConStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\wilDBase.mdf;Integrated Security=True";

        public Form2()
        {
            InitializeComponent();
            button1.Enabled = false;
            int count;
            string path;
            SqlConnection Conn = new SqlConnection(ConStr);
            Conn.Open();
            using (var cmd = Conn.CreateCommand())
            {
                cmd.CommandText = "SELECT  vid_addr FROM  vid WHERE vid_id=1";
                path = cmd.ExecuteScalar().ToString();
            }
            Conn.Close();

            pictureBox1.Image = Image.FromFile(path);
            progressBar1.Maximum = DBcacpacity();
            progressBar1.Value = 0;
            progressBar1.Step = 1;

            
            int wascounted = WaitNoNull();
            do
            {
                count = WaitNoNull();
                wascounted -= count;
                progressBar1.Step = wascounted;
                progressBar1.PerformStep();
                wascounted = count;
                System.Threading.Thread.Sleep(1000);

            } while (count != 0);

            button1.Enabled = true;

        }
        public int WaitNoNull()
        {
            int count2;
            SqlConnection Conn = new SqlConnection(ConStr);
            Conn.Open();
            using (var cmd = Conn.CreateCommand())
            {
                cmd.CommandText = "SELECT  COUNT( * ) FROM  vid WHERE vid_animal IS NULL";
                count2 = Int32.Parse(cmd.ExecuteScalar().ToString());
            }
            Conn.Close();
            return count2;
        }
        public int DBcacpacity()
        {
            int count;
            SqlConnection Conn = new SqlConnection(ConStr);
            Conn.Open();
            using (var cmd = Conn.CreateCommand())
            {
                cmd.CommandText = "SELECT  COUNT( * ) FROM  vid ";
                count = Int32.Parse(cmd.ExecuteScalar().ToString());
            }
            Conn.Close();
            return count;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
