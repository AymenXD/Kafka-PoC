using Consumers.Database;
using Consumers.Models;
using Confluent.Kafka;
using MongoDB.Driver;

namespace Consumers.Services
{
    public class DatabaseService
    {
        private readonly IMongoCollection<KafkaEntry> _collection;
        private readonly ILogger<DatabaseService> _logger;

        public DatabaseService(IMongoClient mongoClient, ILogger<DatabaseService> logger)
        {
            var database = mongoClient.GetDatabase("KafkaData");
            _collection = database.GetCollection<KafkaEntry>("KafkaEntries");
            _logger = logger;
        }

        public async Task SaveDataEntryAsync(KafkaEntry dataEntry)
        {
            await _collection.InsertOneAsync(dataEntry);
            _logger.LogInformation($"Message saved with CompanyId = {dataEntry.CompanyId},  AdditionalData = {dataEntry.AdditionalData}");
        }
    }
}
