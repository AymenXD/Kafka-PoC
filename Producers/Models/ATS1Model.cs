using System.Text.Json.Serialization;

namespace Producers.Models
{
    public class Ats1DataModel
    {
        [JsonPropertyName("device_id")]
        public int DeviceId { get; set; }
        [JsonPropertyName("temperature")]
        public double Temperature { get; set; }
        [JsonPropertyName("humidity")]
        public double Humidity { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
