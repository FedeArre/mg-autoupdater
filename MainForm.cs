using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Diagnostics;
using MyGarage_Autoupdater_Client.JSON_Objects;

namespace MyGarage_Autoupdater_Client
{
    public partial class MainForm : Form
    {
        public static string CurrentVersion = "v1.0.0";
        JSON_AutoupdaterData data;
        bool updatingAutoupdater = false;

        public static string ModsFolderPath = AppContext.BaseDirectory + "..\\";
        public List<ModWrapper> ModInstances;
        public Queue<DownloadData> downloadLinksQueue;
        DownloadData currentDownloadingMod;

        string autoupdaterFolderPath = AppContext.BaseDirectory;
        
        public MainForm()
        {
            ModInstances = new List<ModWrapper>();
            string internalDownloadsFolder = autoupdaterFolderPath + "\\temp_downloads";
            if (!Directory.Exists(internalDownloadsFolder))
                Directory.CreateDirectory(internalDownloadsFolder);

            Logger.WriteLog("MainForm started");
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            var modType = typeof(Mod);

            try
            {
                string[] files = Directory.GetFiles(ModsFolderPath, "*.dll");
                Array.Sort(files, StringComparer.InvariantCulture);
                Assembly asm;

                Console.WriteLine(autoupdaterFolderPath);
                Console.WriteLine(ModsFolderPath);
                for (int i = 0; i < files.Length; i++)
                {
                    try
                    {
                        asm = Assembly.LoadFrom(files[i]);
                        Type[] types = asm.GetTypes();
                        for (int j = 0; j < types.Length; j++)
                        {
                            if (types[j] != null)
                            {
                                if (modType.IsAssignableFrom(types[j]))
                                {
                                    try
                                    {
                                        Mod m = (Mod)FormatterServices.GetUninitializedObject(types[j]);

                                        ModWrapper mw = new ModWrapper();
                                        mw.ID = m.ID;
                                        mw.Name = m.Name;
                                        mw.Version = m.Version;

                                        ModInstances.Add(mw);
                                    }
                                    catch (Exception exx)
                                    {
                                        Logger.WriteLog($"Detected broken assembly ({types[j].FullName}), error: {exx.Message}");
                                        MessageBox.Show($"{types[j].FullName} assembly does not support autoupdating. The mod may require a manual update before being able to update itself.\n\nError: {exx.Message}");
                                    }
                                }
                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        if (ex is ReflectionTypeLoadException)
                        {
                            ReflectionTypeLoadException rtle = (ReflectionTypeLoadException)ex;
                            Type[] types = rtle.Types;

                            for (int j = 0; j < types.Length; j++)
                            {
                                if (types[j] != null)
                                {
                                    if (modType.IsAssignableFrom(types[j]))
                                    {
                                        try
                                        {
                                            Mod m = (Mod)FormatterServices.GetUninitializedObject(types[j]);

                                            ModWrapper mw = new ModWrapper();
                                            mw.ID = m.ID;
                                            mw.Name = m.Name;
                                            mw.Version = m.Version;

                                            ModInstances.Add(mw);
                                        }
                                        catch(Exception exx)
                                        {
                                            Logger.WriteLog($"Detected broken assembly ({types[j].FullName}), error: {exx.Message}");
                                            MessageBox.Show($"{types[j].FullName} assembly does not support autoupdating. The mod may require a manual update before being able to update itself.\n\nError: {exx.Message}");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLog("Fatal error occured on mod load: " + ex.Message);
                MessageBox.Show("A fatal error occurred, please report this!\n" + ex.Message);
            }

            data = APIWrapper.Instance().GetAutoupdaterVersion();
            if(data.current_version != CurrentVersion)
            {
                Logger.WriteLog($"Using outdated Autoupdater version ({CurrentVersion} - available {data.current_version})");
                DialogResult confirmResult = MessageBox.Show($"An update is available for the autoupdater\n\nDo you want to install the update?", "Install", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    UpdateAutoupdater();
                }
            }

            DoModCheck();
        }

        // Autoupdater updating
        public void UpdateAutoupdater()
        {
            string internalDownloadsFolder = autoupdaterFolderPath + "\\temp_downloads";
            if (!Directory.Exists(internalDownloadsFolder))
                Directory.CreateDirectory(internalDownloadsFolder);

            WebClient client = new WebClient();
            client.DownloadProgressChanged += OnAutoupdaterUpdateProgress;
            client.DownloadFileCompleted += OnAutoupdaterUpdateFinish;

            try
            {
                client.DownloadFileAsync(new Uri(data.download_link), internalDownloadsFolder + $"\\Autoupdater.exe");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Autoupdater has an invalid download link. Error: {ex.Message}");
            }
            updatingAutoupdater = true;
        }

        public void OnAutoupdaterUpdateProgress(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            int val = int.Parse(Math.Truncate(percentage).ToString());
            if (currentDownloadingMod != null)
                txt_status.Text = $"Downloading update - {val}%";
        }

        public void OnAutoupdaterUpdateFinish(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                MessageBox.Show("An error ocurred while downloading the file, if this happens again report this!\n\nError: " + e.Error.Message);
                return;
            }

            if (e.Cancelled)
            {
                MessageBox.Show("The current download has been cancelled");
            }

            // Downloads have finished
            Logger.WriteLog("Opening helper with UPDATER tag.");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = autoupdaterFolderPath + "\\AutoupdaterHelper.exe";
            startInfo.Arguments = "UPDATER";
            Process.Start(startInfo);
            System.Environment.Exit(0);
        }

        // Mod updating
        public void DoModCheck()
        {
            if (updatingAutoupdater)
                return;

            var updates = APIWrapper.Instance().GetUpdates(ModInstances);

            if(updates == null)
            {
                MessageBox.Show("Mod check was not succesful. Check for server status.");
                txt_status.Text = "Something went wrong";
                return;
            }
            
            Logger.WriteLog("Updates found: " + updates.Count);

            if (updates.Count == 0)
                txt_status.Text = "All mods are up to date";
            else
            {
                txt_status.Text = $"{updates.Count} ";
                txt_status.Text += updates.Count == 1 ? "mod has an update" : "mods have an update";

                string list = "";
                foreach(var update in updates)
                {
                    list += $"{update.mod_name}, ";
                }

                list = list.Remove(list.Length - 2);

                DialogResult confirmResult = MessageBox.Show($"Updates available for the following mod(s): {list}\n\nDo you want to install them?", "Install", MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    Logger.WriteLog("Starting to download updates");
                    DownloadUpdates();
                }
            }
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            txt_status.Text = "Checking";
            DoModCheck();
        }

        public void DownloadUpdates()
        {
            txt_status.Text = "Retriving data from the database";
            var updates = APIWrapper.Instance().GetUpdates(ModInstances);
            List<DownloadData> download_links = APIWrapper.Instance().GetDownloadLinks(updates);

            downloadLinksQueue = new Queue<DownloadData>();

            using (WebClient wc = new WebClient())
            {
                foreach(DownloadData dd in download_links)
                {
                   downloadLinksQueue.Enqueue(dd);
                }

                StartDownloads();
            }
        }

        public void StartDownloads()
        {
            string internalDownloadsFolder = autoupdaterFolderPath + "\\temp_downloads";
            if (!Directory.Exists(internalDownloadsFolder))
                Directory.CreateDirectory(internalDownloadsFolder);

            if (downloadLinksQueue.Any())
            {
                WebClient client = new WebClient();
                client.DownloadProgressChanged += DownloadProgressChange;
                client.DownloadFileCompleted += DownloadFinished;

                DownloadData dd = downloadLinksQueue.Dequeue();
                currentDownloadingMod = dd;
                try
                {
                    client.DownloadFileAsync(new Uri(dd.Mod_Url), internalDownloadsFolder + $"\\{dd.File_Name}");
                    Logger.WriteLog($"Now downloading: " + dd.Mod_Name);
                } 
                catch(Exception ex)
                {
                    Logger.WriteLog($"Error trying to download a mod, error: " + ex.Message);
                    MessageBox.Show($"{dd.Mod_Name} has an invalid download link. Error: {ex.Message}");
                }
                return;
            }

            // Downloads have finished
            Logger.WriteLog("Opening helper with MODUPDATES tag.");
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = autoupdaterFolderPath + "\\AutoupdaterHelper.exe";
            startInfo.Arguments = "MODUPDATES";
            Process.Start(startInfo);
            System.Environment.Exit(0);
        }

        public void DownloadProgressChange(object sender, DownloadProgressChangedEventArgs e)
        {
            double bytesIn = double.Parse(e.BytesReceived.ToString());
            double totalBytes = double.Parse(e.TotalBytesToReceive.ToString());
            double percentage = bytesIn / totalBytes * 100;
            int val = int.Parse(Math.Truncate(percentage).ToString());
            if(currentDownloadingMod != null)
                txt_status.Text = $"Downloading {currentDownloadingMod.Mod_Name} - {val}%";
        }

        public void DownloadFinished(object sender, AsyncCompletedEventArgs e)
        {
            if(e.Error != null)
            {
                Logger.WriteLog($"Error on mod download finish, error: " + e.Error.ToString());
                MessageBox.Show("An error ocurred while downloading the file, if this happens again report this!\n\nError: " + e.Error.Message);
                return;
            }

            if(e.Cancelled)
            {
                Logger.WriteLog($"Mod download was cancelled");
                MessageBox.Show("The current download has been cancelled");
            }

            StartDownloads();
        }

        // Useful functions
        public static object GetPropertyValue(object o, string propertyName)
        {
            // Get the property we want to access
            PropertyInfo property = o.GetType().GetProperty(propertyName);

            // Retrieve the value of that property in the specified object o
            return property.GetValue(o);
        }
        public static object InvokeMethod(object o, string methodName, object[] arguments)
        {
            // First lets generate a Type[] for the parameters.  To do this we use some
            // LINQ to select the type of each element in the arguments array
            Type[] types = arguments.Select(x => x.GetType()).ToArray();

            // Get the MethodInfo for the method for the object specified
            MethodInfo method = o.GetType().GetMethod(methodName, types);

            // Invoke the method on the object we passed and return the result.
            return method.Invoke(o, arguments);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Process.Start("steam://rungameid/1578390");
            System.Environment.Exit(0);
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"My Garage Autoupdater\nVersion: {CurrentVersion}\n\nDeveloped by Federico Arredondo", "About");
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Error");
        }
    }
}
