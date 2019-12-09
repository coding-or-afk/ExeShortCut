using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Configuration;
using System.Collections.Specialized;

namespace sdwExeShortCut
{
    public partial class Form1 : Form
    {
        public string mPropertyValue = String.Empty;

        public Form1()
        {
            InitializeComponent();
            InitializeByConfig();
        }
        
        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.InitialDirectory = "C:\\";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                ListViewItem lvi = new ListViewItem();
                Icon icon = Icon.ExtractAssociatedIcon(ofd.FileName);
                Image iconImg = icon.ToBitmap();
                imageList1.Images.Add(iconImg);
                lvi.ImageIndex = imageList1.Images.Count-1;
                lvi.Text = ofd.SafeFileName;
                lvi.Tag = ofd.FileName;
                lvExplorer.Items.Add(lvi);

                saveConfig(ofd.SafeFileName,ofd.FileName);
            }
            else
            {
                MessageBox.Show("Error Occured When Opening File");
                return;
            }
        }
        
        private void lvExplorer_DoubleClick(object sender, EventArgs e)
        {
            string excPath = Convert.ToString(lvExplorer.SelectedItems[0].Tag);
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = excPath;
            proc.StartInfo.CreateNoWindow = false;
            try
            {
                proc.Start();
            }
            catch(Exception error)
            {
                MessageBox.Show(error.Message);
                return;
            }
            
        }

        private void btnDir_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            DialogResult result = fbd.ShowDialog();
            if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
            {
                string[] files = Directory.GetFiles(fbd.SelectedPath);

                for (int i = 0; i < files.Length; i++)
                {
                    if(files[i].Length < 4)
                    {
                        return;
                    }

                    if(files[i].Substring(files[i].Length - 4, 4) == ".exe")
                    {
                        ListViewItem lvi = new ListViewItem();
                        Icon icon = Icon.ExtractAssociatedIcon(files[i]);
                        Image iconImg = icon.ToBitmap();
                        imageList1.Images.Add(iconImg);
                        lvi.ImageIndex = imageList1.Images.Count - 1;
                        lvi.Text = strchr(files[i],'\\');
                        lvi.Tag = files[i];
                        lvExplorer.Items.Add(lvi);

                        saveConfig(lvi.Text,files[i]);
                    }
                }
                return;
            }
            else
            {
                MessageBox.Show("Directory Not Found");
                return;
            }
        }

        private string strchr(string origin, char charToSearch)
        {
            int? found = origin.LastIndexOf(charToSearch);
            if(found > -1)
            {
                string retval = origin.Substring((int)found+1, origin.Length - (int)found-1);
                return retval;
            }
            else
            {
                return null;
            }
        }

        private void lvExplorer_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            // 선택된 아이템이 없을때
            if (lvExplorer.SelectedItems.Count < 1)
            {
                return;
            }

            //오른쪽 더블클릭
            if(e.Button == MouseButtons.Right)
            {
                return;
            }

            string excPath = Convert.ToString(lvExplorer.SelectedItems[0].Tag);
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = excPath;
            try
            {
                proc.Start();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                return;
            }
        }

        private void lvExplorer_MouseClick(object sender, MouseEventArgs e)
        {
            //선택된 아이템이 없을때
            if(lvExplorer.SelectedItems.Count < 1)
            {
                return;
            }

            //오른쪽 버튼 클릭
            if (e.Button == MouseButtons.Right)
            {
                if (lvExplorer.FocusedItem.Bounds.Contains(e.Location))
                {
                    contextMenuStrip1.Show(Cursor.Position);
                }
            }
        }

        private void uACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string excPath = Convert.ToString(lvExplorer.SelectedItems[0].Tag);
            Process proc = new Process();
            proc.StartInfo.FileName = excPath;
            proc.StartInfo.UseShellExecute = true;
            if (System.Environment.OSVersion.Version.Major >= 6)
            {
                proc.StartInfo.Verb = "runas";
            }
            try
            {
                proc.Start();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                return;
            }
        }
        private void nonUACToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string excPath = Convert.ToString(lvExplorer.SelectedItems[0].Tag);
            Process proc = new Process();
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.FileName = excPath;
            try
            {
                proc.Start();
            }
            catch (Exception error)
            {
                MessageBox.Show(error.Message);
                return;
            }
        }

        public void InitializeByConfig()
        {
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                for(int i=0; i< appSettings.Count; i++)
                {
                    string filepath = appSettings[i];
                    ListViewItem lvi = new ListViewItem();
                    Icon icon = Icon.ExtractAssociatedIcon(filepath);
                    Image iconImg = icon.ToBitmap();
                    imageList1.Images.Add(iconImg);
                    lvi.ImageIndex = imageList1.Images.Count - 1;
                    lvi.Text = strchr(filepath, '\\');
                    lvi.Tag = filepath;
                    lvExplorer.Items.Add(lvi);
                }
            }
            catch (ConfigurationException err)
            {
                MessageBox.Show(err.Message);
            }
        }

        public void saveConfig(string key, string path)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var appSettings = configFile.AppSettings.Settings;
                if (appSettings[key] == null)
                {
                    appSettings.Add(key, path);
                }
                else
                {
                    MessageBox.Show("Already Exists!");
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException err)
            {
                MessageBox.Show(err.Message);
            }
        }

        public void deleteConfig(string key)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var appSettings = configFile.AppSettings.Settings;
                if (appSettings[key] == null)
                {
                    MessageBox.Show("Error in Deleting Config File");
                }
                else
                {
                    appSettings.Remove(key);
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException err)
            {
                MessageBox.Show(err.Message);
            }
            
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            deleteItem();
        }

        private void deleteItem()
        {
            
            if(lvExplorer.SelectedItems.Count > 0)
            {
                string key = strchr((string)lvExplorer.SelectedItems[0].Tag, '\\');
                deleteConfig(key);
                lvExplorer.Items.Remove(lvExplorer.SelectedItems[0]);
            }
            //선택된 아이템이 없음
            else
            {
                MessageBox.Show("No Items Selected");
                return;
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            deleteItem();
        }
    }
}
