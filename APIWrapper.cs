using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Net;
using MyGarage_Autoupdater_Client.JSON_Objects;
using System.IO;

namespace MyGarage_Autoupdater_Client
{
    internal class APIWrapper
    {
        private static APIWrapper instance;

        public const string API_URL = "http://localhost:3000";

        private APIWrapper()
        {
            Logger.WriteLog("API Wrapper started, using URL " + API_URL);
        }

        public static APIWrapper Instance()
        {
            if (instance == null)
                instance = new APIWrapper();
            return instance;
        }

        public List<JSON_Mod_API_Result> GetUpdates(List<ModWrapper> modList)
        {
            JSON_ModList jsonList = new JSON_ModList();

            foreach(ModWrapper mod in modList)
            {
                JSON_Mod jsonMod = new JSON_Mod();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_URL + "/mods");
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Accept = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(JsonConvert.SerializeObject(jsonList));
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();

                    List<JSON_Mod_API_Result> jsonObj = JsonConvert.DeserializeObject<List<JSON_Mod_API_Result>>(result);
                    return jsonObj;
                }
            }
            catch(Exception ex)
            {
                Logger.WriteLog("Error ocurred on GetUpdates, error: " + ex.ToString());
                MainForm.ShowError("Error while trying to get the updates. Information available on the log.");
                return null;
            }
        }

        public List<DownloadData> GetDownloadLinks(List<JSON_Mod_API_Result> mods)
        {
            List<DownloadData> links = new List<DownloadData>();

            foreach(JSON_Mod_API_Result mod in mods)
            {
                try
                {
                    var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_URL + "/mod_download");
                    httpWebRequest.ContentType = "application/json";
                    httpWebRequest.Accept = "application/json";
                    httpWebRequest.Method = "POST";

                    using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                    {
                        streamWriter.Write($"{{ \"mod_id\": \"{mod.mod_id}\"}}");
                    }

                    var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                    using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        var result = streamReader.ReadToEnd();

                        links.Add(new DownloadData(mod.mod_name, result, mod.file_name));
                    }
                } 
                catch(Exception ex)
                {
                    Logger.WriteLog("Error ocurred on GetDownloadLinks, error: " + ex.ToString());
                    MainForm.ShowError("Error while trying to get download links. Information available on the log.");
                }
            }

            return links;
        }

        public JSON_AutoupdaterData GetAutoupdaterVersion()
        {
            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_URL + "/autoupdater");
                httpWebRequest.ContentType = "application/json";

                using(HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (Stream stream = response.GetResponseStream())
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        string json = reader.ReadToEnd();
                        JSON_AutoupdaterData jsonObject = JsonConvert.DeserializeObject<JSON_AutoupdaterData>(json);

                        return jsonObject;
                    }
                }
            } 
            catch(Exception ex)
            {
                Logger.WriteLog("Error ocurred on GetAutoupdaterVersion, error: " +ex.ToString());
                MainForm.ShowError("Error while trying to get the autoupdater version. Information available on the log.");
                return null;
            }
        }
    }
}
