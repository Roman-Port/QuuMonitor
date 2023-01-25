using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuuEmbeddedPreview
{
    class PreviewConfig
    {
        // GENERAL SETTINGS

        [JsonProperty("quu_config_file")]
        public string QuuConfigFile { get; set; }

        [JsonProperty("cache_directory")]
        public string CacheDirectory { get; set; }

        [JsonProperty("display_width")]
        public int DisplayWidth { get; set; }

        [JsonProperty("display_height")]
        public int DisplayHeight { get; set; }

        // STATION SETTINGS

        [JsonProperty("quu_station_id")]
        public string QuuStationId { get; set; }

        [JsonProperty("call_letters")]
        public string CallLetters { get; set; }

        [JsonProperty("branding")]
        public string Branding { get; set; }
    }
}
