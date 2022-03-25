using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoupdaterHelper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            System.Threading.Thread.Sleep(1500);
            if (args.Length == 0)
            {
                System.Environment.Exit(0);
                return;
            }

            string currentPath = AppContext.BaseDirectory;
            string modsFolder = currentPath + "..\\";
            string downloadsFolder = currentPath + "temp_downloads\\";

            if (args[0] == "MODUPDATES")
            {
                if (Directory.Exists(downloadsFolder))
                {
                    foreach (var file in Directory.EnumerateFiles(downloadsFolder))
                    {
                        string destFile = Path.Combine(modsFolder, Path.GetFileName(file));

                        if (File.Exists(destFile))
                            File.Delete(destFile);

                        File.Move(file, destFile);
                    }
                }
            }

            if(args[0] == "UPDATER")
            {
                string file = Path.Combine(downloadsFolder, "Autoupdater.exe");
                if (File.Exists(file))
                {
                    string destFile = Path.Combine(currentPath, "Autoupdater.exe");
                    File.Delete(destFile);
                    File.Move(file, destFile);
                }
            }

            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = currentPath + "\\Autoupdater.exe";
            startInfo.Arguments = "log_restart_disable";
            Process.Start(startInfo);
        }
    }
}
