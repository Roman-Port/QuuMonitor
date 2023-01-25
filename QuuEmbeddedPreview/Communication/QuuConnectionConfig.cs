using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview.Communication
{
    class QuuConnectionConfig
    {
        [JsonProperty("ws_url")]
        public string WsUrl { get; set; }
    }
}
