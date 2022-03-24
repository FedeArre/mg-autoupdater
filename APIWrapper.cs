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
        private static readonly HttpClient client = new HttpClient();

        public const string API_URL = "http://localhost:3000";

        private APIWrapper()
        {

        }

        public static APIWrapper Instance()
        {
            if (instance == null)
                instance = new APIWrapper();
            return instance;
        }

        public List<JSON_Mod_API_Result> GetUpdates(List<ModWrapper> modList)
        {
            List<string> updates = new List<string>();
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
                return null;
            }
        }

        public List<string> GetDownloadLinks(List<JSON_Mod_API_Result> mods)
        {
            List<string> links = new List<string>();

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

                        links.Add(result);
                    }
                } 
                catch(Exception ex)
                {
                    Console.Write("a");
                    // TODO: Logger
                }
            }

            return links;
        }
    }
}
