using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
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
            treeView1.BeforeSelect += treeView1_BeforeSelect;
            treeView1.BeforeExpand += treeView1_BeforeExpand;
            // заполняем дерево дисками
            FillDriveNodes();


            //PopulateTreeView1();
        }
        public void PyLaunch()
        {
            ScriptEngine engine = Python.CreateEngine();
            string filename = @"...\main.py";  //ФАЙЛ В ПАПКУ БИН
            Task.Factory.StartNew(() => engine.ExecuteFile(filename));
        }
        public void InsertFiles(string path)         //Инсертит в БД
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            List<string> files = new List<string>();
            foreach (FileInfo file in dir.GetFiles())                               //Перебираем файлы и запихиваем Адрес в Лист
            {   if (file.Name.ToLowerInvariant().Contains(".avi") ||
                    file.Name.ToLowerInvariant().Contains(".mov") ||
                    file.Name.ToLowerInvariant().Contains(".jpg") ||
                    file.Name.ToLowerInvariant().Contains(".jpeg"))
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

        /// <summary>
        /// получает папки для рабочего стола в Tree
        /// </summary>
        private void FillDriveNodes()
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    TreeNode driveNode = new TreeNode { Text = drive.Name };
                    FillTreeNode(driveNode, drive.Name);
                    treeView1.Nodes.Add(driveNode);
                }
            }
            catch (Exception ex) { }
        }
        void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string[] dirs;
            try
            {
                if (Directory.Exists(e.Node.FullPath))
                {
                    dirs = Directory.GetDirectories(e.Node.FullPath);
                    if (dirs.Length != 0)
                    {
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            TreeNode dirNode = new TreeNode(new DirectoryInfo(dirs[i]).Name);
                            FillTreeNode(dirNode, dirs[i]);
                            e.Node.Nodes.Add(dirNode);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        // событие перед выделением узла
        void treeView1_BeforeSelect(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear();
            string[] dirs;
            try
            {
                if (Directory.Exists(e.Node.FullPath))
                {
                    dirs = Directory.GetDirectories(e.Node.FullPath);
                    if (dirs.Length != 0)
                    {
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            TreeNode dirNode = new TreeNode(new DirectoryInfo(dirs[i]).Name);
                            FillTreeNode(dirNode, dirs[i]);
                            e.Node.Nodes.Add(dirNode);
                        }
                    }
                }
            }
            catch (Exception ex) { }
        }
        private void FillTreeNode(TreeNode driveNode, string path)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);
                foreach (string dir in dirs)
                {
                    TreeNode dirNode = new TreeNode();
                    dirNode.Text = dir.Remove(0, dir.LastIndexOf("\\") + 1);
                    driveNode.Nodes.Add(dirNode);
                }
            }
            catch (Exception ex) { }
        }

        public string fullPath = "";


        /// <summary>
        /// получает путь выбранного treeview элементa
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selectedNode = e.Node;
            fullPath = selectedNode.FullPath;
        }
        


        /// <summary>
        /// получает полный путь, прибавляя имя выбраного файла в listview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listView1_AfterClick(object sender, EventArgs e)
        {
            button1.Enabled = true;
            fullPath += "\\" + listView1.SelectedItems[0].Text;
        }



        /// <summary>
        /// обработчик события кнопки
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button1_Click(object sender, EventArgs e)
        {
            string path = null;
            path = fullPath;

            InsertFiles(path); // Инсертит в БД

            PyLaunch();        // Запускет питоновский файл из Bin

            int allDB = DBcacpacity();
            int count;
            do
            {
                count = WaitNoNull();
                System.Threading.Thread.Sleep(1000);

            } while (count != 0);


            SortFiles(path);   // Запускает пост-обработочную сортировку

        }
       
    }
}
