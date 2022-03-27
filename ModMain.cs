using Autoupdater.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

namespace Autoupdater
{
    public class ModMain : Mod
    {
        // Looking for docs? https://fedearre.github.io/my-garage-modding-docs/
        public override string ID => "ModAutoupdater";
        public override string Name => "Autoupdater";
        public override string Author => "Federico Arredondo";
        public override string Version => "v1.0.0-rc1";

        const string API_URL = "https://mygaragemod.xyz";
        GameObject UI_Prefab, UI_Error_Prefab, UI;

        bool MenuFirstLoad = false;

        public ModMain()
        {
            Debug.Log("Loading Autoupdater mod, version: " + Version);
            AssetBundle Bundle = AssetBundle.LoadFromMemory(Properties.Resources.autoupdater_ui_canvas);
            UI_Prefab = Bundle.LoadAsset<GameObject>("Canvas");
            UI_Error_Prefab = Bundle.LoadAsset<GameObject>("CanvasError");
            UI_Prefab.GetComponent<Canvas>().sortingOrder = 1; // Fixes canva disappearing after a bit.
            UI_Error_Prefab.GetComponent<Canvas>().sortingOrder = 1;
        }

        public override void OnMenuLoad()
        {
            if (!MenuFirstLoad)
            {
                MenuFirstLoad = true;
                return;
            }

            JSON_ModList jsonList = new JSON_ModList();
            foreach (Mod mod in ModLoader.mods)
            {
                JSON_Mod jsonMod = new JSON_Mod();

                jsonMod.modId = mod.ID;
                jsonMod.version = mod.Version;

                jsonList.mods.Add(jsonMod);
            }

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(API_URL + "/mods");
                Debug.LogError(API_URL);
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

                    if(jsonObj.Count > 0)
                    {
                        // Updates available.
                        UI = GameObject.Instantiate(UI_Prefab);
                        foreach (Button btt in UI.GetComponentsInChildren<Button>())
                        {
                            if (btt.name == "ButtonNo")
                            {
                                btt.onClick.AddListener(UI_ButtonNo);
                            }
                            else if (btt.name == "ButtonYes")
                            {
                                btt.onClick.AddListener(UI_ButtonYes);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error occured while trying to fetch updates, error: " + ex.ToString());
                GameObject.Instantiate(UI_Error_Prefab);
            }
        }

        public void UI_ButtonNo()
        {
            if (UI)
                GameObject.Destroy(UI);
        }

        public void UI_ButtonYes()
        {
            string autoupdaterPath = Path.Combine(Application.dataPath, "..\\Mods\\Autoupdater\\Autoupdater.exe");
            Debug.Log("UI button yes: Path is " + autoupdaterPath);

            if (File.Exists(autoupdaterPath))
            {
                GameObject.Destroy(UI);
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = autoupdaterPath;
                Process.Start(startInfo);
                Application.Quit(0);
            }
        }
    }
}