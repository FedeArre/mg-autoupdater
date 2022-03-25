using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGarage_Autoupdater_Client
{
    internal class Logger
    {
        public static void LoggerStart()
        {
            string logFilePath = Path.Combine(AppContext.BaseDirectory, "log.txt");
            if(File.Exists(logFilePath))
                File.Delete(logFilePath);
        }

        public static void WriteLog(string strLog)
        {
            StreamWriter log;
            FileStream fileStream = null;
            DirectoryInfo logDirInfo = null;
            FileInfo logFileInfo;

            string logFilePath = Path.Combine(AppContext.BaseDirectory, "log.txt");
            logFileInfo = new FileInfo(logFilePath);
            logDirInfo = new DirectoryInfo(logFileInfo.DirectoryName);
            if (!logDirInfo.Exists) logDirInfo.Create();
            if (!logFileInfo.Exists)
            {
                fileStream = logFileInfo.Create();
            }
            else
            {
                fileStream = new FileStream(logFilePath, FileMode.Append);
            }
            log = new StreamWriter(fileStream);
            log.WriteLine("[LOG]: " + strLog);
            log.Close();
        }
    }
}
