using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Consumers.Models
{
    [Table("KafkaEntries")]
    public class KafkaEntry
    {
        [BsonId]
        public ObjectId Id { get; set; }

        public string CompanyId { get; set; }

        [BsonExtraElements]
        public BsonDocument AdditionalData { get; set; }
    }
}
