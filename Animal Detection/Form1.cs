﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Animal_Detection
{
   
    public partial class Form1 : Form
    {
        public string ConStr = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\wilDBase.mdf;Integrated Security=True";


        public Form1()
        {
            
            InitializeComponent();
            PopulateTreeView();
            

            // PopulateTreeView1();
        }
        public void PyLaunch()
        {
            ScriptEngine engine = Python.CreateEngine();
            string filename = @"...\main.py";  //ФАЙЛ В ПАПКУ БИН
            engine.ExecuteFile(filename);
        }
        public void InsertFiles(string path)         //Инсертит в БД
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> files = new List<string>();
            foreach (FileInfo file in dir.GetFiles())                               //Перебираем файлы и запихиваем Адрес в Лист
            {
                files.Add(file.FullName);
            }


            SqlConnection Conn = new SqlConnection(ConStr);
            Conn.Open();
            foreach (string filename in files)
            {
                using (var cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = @"INSERT  vid (vid_addr) Values (@filename)";
                    cmd.Parameters.AddWithValue("@filename", filename);
                    cmd.ExecuteNonQuery();
                }
            }
            Conn.Close();
        }




        public void SortFiles(string path)
        {
            SqlConnection Conn = new SqlConnection(ConStr);
            Conn.Open();
            int count;
            using (var cmd = Conn.CreateCommand())
            {
                cmd.CommandText = "SELECT  COUNT( * ) FROM  vid";
                count = Int32.Parse(cmd.ExecuteScalar().ToString());
            }

            string goodPath = path + @"\Good";
            string badPath = path + @"\Bad";
            string soPath = path + @"\So-so";
            if (!Directory.Exists(goodPath)) Directory.CreateDirectory(goodPath);
            if (!Directory.Exists(badPath)) Directory.CreateDirectory(badPath);
            if (!Directory.Exists(soPath)) Directory.CreateDirectory(soPath);
            for (int i = 1; i <= count; i++)
            {
                using (var cmd = Conn.CreateCommand())
                {
                    cmd.CommandText = "SELECT  vid_animal FROM  vid WHERE vid_id=@id";
                    cmd.Parameters.AddWithValue("@id", i);
                    char p = cmd.ExecuteScalar().ToString().First();

                    cmd.CommandText = "SELECT  vid_addr FROM  vid WHERE vid_id=@id";
                    cmd.Parameters.AddWithValue("@id", i);
                    string addr = cmd.ExecuteScalar().ToString();
                    FileInfo fileInf = new FileInfo(path);

                    switch (p)
                    {
                        case 'T':
                            if (fileInf.Exists) fileInf.CopyTo(goodPath);
                            break;
                        case 'F':
                            if (fileInf.Exists) fileInf.CopyTo(badPath);
                            break;
                        case 'S':
                            if (fileInf.Exists) fileInf.CopyTo(soPath);
                            break;
                        default:
                            break;
                    }
                }
            }

            using (var cmd = Conn.CreateCommand())
            {
                cmd.CommandText = "TRUNCATE TABLE vid";
                cmd.ExecuteNonQuery();
            }

            Conn.Close();
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

        private void PopulateTreeView()
        {
            TreeNode rootNode;
            string path = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }
        private void PopulateTreeView1()
        {
            TreeNode rootNode;
            string path = @"C:\\";
            DirectoryInfo info = new DirectoryInfo(path);
            if (info.Exists)
            {
                rootNode = new TreeNode(info.Name);
                rootNode.Tag = info;
                GetDirectories(info.GetDirectories(), rootNode);
                treeView1.Nodes.Add(rootNode);
            }
        }

        private void GetDirectories(DirectoryInfo[] subDirs,
            TreeNode nodeToAddTo)
        {
            TreeNode aNode;
            DirectoryInfo[] subSubDirs;
            
            foreach (DirectoryInfo subDir in subDirs)
            {
                aNode = new TreeNode(subDir.Name, 0, 0);
                aNode.Tag = subDir;
                aNode.ImageKey = "folder";
                subSubDirs = subDir.GetDirectories();
                if (subSubDirs.Length != 0)
                {
                     GetDirectories(subSubDirs, aNode);
                }
                nodeToAddTo.Nodes.Add(aNode);

            }
        }
        void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            TreeNode newSelected = e.Node;
            listView1.Items.Clear();
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item = null;

            foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
            {
                item = new ListViewItem(dir.Name, 0);
                subItems = new ListViewItem.ListViewSubItem[]
                    {new ListViewItem.ListViewSubItem(item, "Directory"),
             new ListViewItem.ListViewSubItem(item,
                dir.LastAccessTime.ToShortDateString())};
                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }
            foreach (FileInfo file in nodeDirInfo.GetFiles())
            {
                item = new ListViewItem(file.Name, 1);
                subItems = new ListViewItem.ListViewSubItem[]
                    { new ListViewItem.ListViewSubItem(item, "File"),
             new ListViewItem.ListViewSubItem(item,
                file.LastAccessTime.ToShortDateString())};

                item.SubItems.AddRange(subItems);
                listView1.Items.Add(item);
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            string path = null;
            path = listView1.SelectedItems[0].Text;

            InsertFiles(path); // Инсертит в БД

            PyLaunch();        // Запускет питоновский файл из Bin

            int count;
            do
            {
                count = WaitNoNull();
            } while (count != 0);


            SortFiles(path);   // Запускает пост-обработочную сортировку

        }
    }
}
