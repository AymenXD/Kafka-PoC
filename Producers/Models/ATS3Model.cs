using System.Text.Json.Serialization;

namespace Producers.Models
{
    public class Ats3DataModel
    {
        [JsonPropertyName("device_id")]
        public int DeviceId { get; set; }
        [JsonPropertyName("latitude")]
        public double Latitude { get; set; }
        [JsonPropertyName("longitude")]
        public double Longitude { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
