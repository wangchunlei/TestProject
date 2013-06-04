using Newtonsoft.Json;

namespace Domas.DAP.IDE.LicenseManager.Models
{
    public class Image
    {
        [JsonProperty(PropertyName = "#text")]
        public string text { get; set; }
        public string size { get; set; }
    }
}