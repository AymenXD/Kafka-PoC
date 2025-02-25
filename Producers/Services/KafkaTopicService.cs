using Confluent.Kafka.Admin;
using Confluent.Kafka;

namespace Producers.Services
{
    public class KafkaTopicService
    {
        private readonly AdminClientConfig config;
        private readonly ILogger<KafkaTopicService> _logger;
        private readonly string[] _topics;

        public KafkaTopicService(string caCertPath, string certPath, string certPassword, string bootstrapServers, ILogger<KafkaTopicService> logger, string[] topics)
        {
            _logger = logger;
            _topics = topics;
            config = new AdminClientConfig
            {
                BootstrapServers = bootstrapServers,
                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = caCertPath,
                SslKeystoreLocation = certPath,
                SslKeyPassword = certPassword,
                SslKeystorePassword = certPassword,
                SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
                Debug = "security"
            };
        }

        public async Task CreateTopicsAsync()
        {
            _logger.LogInformation($"KafkaTopicService is starting ...");
            _logger.LogInformation($"Wanted Topics to create: {string.Join(", ", _topics)}");

            using (var adminClient = new AdminClientBuilder(config).Build())
            {
                try
                {
                    var topicSpecifications = new List<TopicSpecification>();

                    foreach (var topic in _topics)
                    {
                        var topicSpecification = new TopicSpecification
                        {
                            Name = topic,
                            NumPartitions = 5, // Set the number of partitions for each topic
                            ReplicationFactor = 3, // Set the replication factor for each topic
                            /*Configs = new Dictionary<string, string>
                                {
                                    { "retention.ms", "604800000" }, // 7 days in milliseconds
                                    { "retention.bytes", "1073741824" } // 1GB of data per partition
                                }*/
                        };
                        topicSpecifications.Add(topicSpecification);
                    }
                    var existingTopics = adminClient.GetMetadata(TimeSpan.FromSeconds(10)).Topics.Select(t => t.Topic).ToList();
                    _logger.LogInformation($"Existing Topics ... {string.Join(", ", existingTopics)}, {existingTopics}");

                    var topicsToCreate = topicSpecifications.Where(topic => !existingTopics.Contains(topic.Name)).ToList();

                    if (topicsToCreate.Any())
                    {
                        await adminClient.CreateTopicsAsync(topicsToCreate);
                        _logger.LogInformation("Kafka topics created successfully: {Topics}", string.Join(", ", topicSpecifications.Select(t => t.Name)));
                    }
                    else
                    {
                        _logger.LogInformation("All Kafka topics already exist.");
                    }
                }
                catch (CreateTopicsException ex)
                {
                    foreach (var result in ex.Results)
                    {
                        _logger.LogError($"An error occurred creating Kafka topic {result.Topic}: {result.Error.Reason}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"An unexpected error occurred during topic creation: {ex.Message}");
                }
            }
        }
    }
}
