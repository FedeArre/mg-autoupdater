using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGarage_Autoupdater_Client.JSON_Objects
{
    internal class JSON_Mod_API_Result
    {
        public int internal_id { get; set; }
        public string mod_id { get; set; }
        public string mod_name { get; set; }
        public string current_version { get; set; }
        public string current_download_link { get; set; }
        public string created_by { get; set; }
        public DateTime last_update { get; set; }
    }
}
