using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Animal_Detection
{
    public partial class Form3 : Form1
    {
        public Form3()
        {
            InitializeComponent();
        }


        private void Button1_Click(object sender, EventArgs e)
        {
            string path = null;
            path = fullPath;
            SortFiles(path);

            // Запускает пост-обработочную сортировку

        }
        private void Form3_Load(object sender, EventArgs e)
        {

        }
    }
}
