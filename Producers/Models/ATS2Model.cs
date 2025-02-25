using System.Text.Json.Serialization;

namespace Producers.Models
{
    public class Ats2DataModel
    {
        [JsonPropertyName("device_id")]
        public int DeviceId { get; set; }
        [JsonPropertyName("signal")]
        public double Signal { get; set; }
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
    }
}
