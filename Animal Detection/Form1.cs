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
using System.Diagnostics;
namespace Animal_Detection
{
   
    public partial class Form1 : Form
    {                                                                                                                        
       
        string fpath = @"...\hta.txt";
        string fpathres = @"...\res.txt";


        public Form1()
        {
            
            InitializeComponent();
            treeView1.BeforeSelect += treeView1_BeforeSelect;
            treeView1.BeforeExpand += treeView1_BeforeExpand;
            // заполняем дерево дисками
            FillDriveNodes();

            
        }
        //public void exeLaunch()
        //{
         //   string filename = @"D:\Документы\MihaREP\wildhack\Animal Detection\frames_from_video2\dist\video_scan.exe";
         //   Process.Start(filename);
        //}
        public void PyLaunch()
        {

            ScriptEngine engine = Python.CreateEngine();
            string filename = @"...\...\main.py";  //ФАЙЛ В ПАПКУ БИН
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
                    file.Name.ToLowerInvariant().Contains(".jpeg") ||
                    file.Name.ToLowerInvariant().Contains(".png"))
                files.Add(file.FullName);
            }

            FileInfo fileInf = new FileInfo(fpath);
            if (fileInf.Exists)
            {
                fileInf.Delete();
            } 
            
            //System.IO.File.Create(fpath);
            StreamWriter f = new StreamWriter(fpath);
            int id = 0;
            foreach (string filename in files)
            {
                string res = id.ToString() + "|"+ filename+"|";
                f.WriteLine(res);
                id++;
            }
            f.Close();
    
        }




        public void SortFiles(string path)
        {
    
            int count= System.IO.File.ReadAllLines(fpathres).Length;
            string goodPath = path + @"\Good";
            string badPath = path + @"\Bad";
            string soPath = path + @"\So-so";
            if (!Directory.Exists(goodPath)) Directory.CreateDirectory(goodPath);
            if (!Directory.Exists(badPath)) Directory.CreateDirectory(badPath);
            if (!Directory.Exists(soPath)) Directory.CreateDirectory(soPath);
            foreach (string str in System.IO.File.ReadLines(fpathres))
            {
                string sub = str.Trim();
                string[] words = str.Split(new char[] { '|' });
                string p = words[2];
                string addr = words[1];
                FileInfo fileInf = new FileInfo(addr);

                    switch (p)
                    {
                        case "T":
                            if (fileInf.Exists) fileInf.CopyTo(goodPath);
                            break;
                        case "F":
                            if (fileInf.Exists) fileInf.CopyTo(badPath);
                            break;
                        case "S":
                            if (fileInf.Exists) fileInf.CopyTo(soPath);
                            break;
                        default:
                            break;
                    }
            }

            FileInfo fileInf1 = new FileInfo(fpath);
            if (fileInf1.Exists)
            {
                fileInf1.Delete();
            }
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
            ListViewItem item = null;
            
            e.Node.Nodes.Clear();
            listView1.Items.Clear();
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
                            item = new ListViewItem(new DirectoryInfo(dirs[i]).Name, 0);
                            
                              
                            listView1.Items.Add(item);
                        }
                    }
                }
            }
            catch (Exception ex) { }

            

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
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
            var NewSelected = ((ListView)sender);
            button1.Enabled = true;
            fullPath += "\\" + NewSelected.FocusedItem.Text;
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
            //exeLaunch();
            Form2 form2=new Form2(); 
            form2.Show(); // Запускет питоновский файл из Bin
            form2.PB();
            this.Visible = false;
            FileInfo fileInf12 = new FileInfo(fpathres);

            while (!fileInf12.Exists)
            {
                System.Threading.Thread.Sleep(1000);
                fileInf12 = new FileInfo(fpathres);
            }
            form2.buttON();

            FileInfo fileInf1 = new FileInfo(fpath);
            while (!fileInf1.Exists&&form2.IsDisposed)
            {
                System.Threading.Thread.Sleep(1000);
            }
            Form3 form3 = new Form3(); ;
            form3.Show();

               // Запускает пост-обработочную сортировку

        }
        public void загрузитьПапкуToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    fullPath = "";
                    string path_1 = dialog.SelectedPath;
                    string f = "Папка:   ";
                    textBox1.Text = f + System.IO.Path.GetFileName(dialog.SelectedPath).ToString();
                    fullPath = path_1;
                    button1.Enabled = true;
                }
            }

        }
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (this.WindowState == System.Windows.Forms.FormWindowState.Maximized)
            {
                this.tableLayoutPanel1.Size = ClientSize;
            }
            if (this.WindowState == System.Windows.Forms.FormWindowState.Minimized)
            {
                this.tableLayoutPanel1.Size = ClientSize;
            }

        }


        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        
    }
}
