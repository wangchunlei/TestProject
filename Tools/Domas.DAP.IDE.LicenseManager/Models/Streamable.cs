using Newtonsoft.Json;

namespace Domas.DAP.IDE.LicenseManager.Models
{
    public class Streamable
    {
        [JsonProperty("#text")]
        public string text { get; set; }
        public string fulltrack { get; set; }
    }
}