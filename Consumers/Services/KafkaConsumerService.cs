using Consumers.Database;
using Consumers.Models;
using Confluent.Kafka;
using System.Text.Json;
using MongoDB.Bson;
using Prometheus;

namespace Consumers.Services
{
    public class KafkaConsumerService : BackgroundService
    {
        private readonly ILogger<KafkaConsumerService> _logger;
        private readonly IConsumer<string, string> _consumer;
        private readonly string[] _topics;
        //private readonly AppDbContext _appDbContext;
        //private readonly DatabaseService _databaseService;
        private readonly IServiceProvider _serviceProvider;

        // Define custom metrics
        private static readonly Counter _messagesReceivedCounter = Metrics.CreateCounter("kafka_messages_received_total", "Total number of messages received from Kafka.");
        private static readonly Histogram _producerToConsumerLatencyHistogram = Metrics.CreateHistogram("producer_to_consumer_latency_seconds", "Time taken from ASP.NET producer to ASP.NET consumer",
            new HistogramConfiguration
            {
                Buckets = Histogram.LinearBuckets(start: 0.001, width: 0.005, count: 10)
            });

        public KafkaConsumerService(string caCertPath, string certPath, string certPassword, ILogger<KafkaConsumerService> logger, string bootstrapServers, string groupId, string[] topics, IServiceProvider serviceProvider)
        {
            //_databaseService = dbService; 
            _serviceProvider = serviceProvider;
            var config = new ConsumerConfig
            {
                BootstrapServers = bootstrapServers,
                GroupId = groupId,
                SecurityProtocol = SecurityProtocol.Ssl,
                SslCaLocation = caCertPath,
                SslKeystoreLocation = certPath,
                SslKeyPassword = certPassword,
                SslKeystorePassword = certPassword,
                SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = true, // Enable auto commit
                AutoCommitIntervalMs = 1000,
                Debug = "security"
            };

            _consumer = new ConsumerBuilder<string, string>(config).Build();
            _topics = topics;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.Run(async () =>
            {
                _logger.LogInformation("KafkaConsumer is starting... ");
                _logger.LogInformation($"Topics to subscribe: {string.Join(", ", _topics)}");

                _consumer.Subscribe(_topics);

                try
                {
                    while (!stoppingToken.IsCancellationRequested)
                    {
                        var result = _consumer.Consume(stoppingToken);

                        if (result != null)
                        {
                            _logger.LogInformation($"Received message '{result.Message.Value}' at: '{result.TopicPartitionOffset}'.");

                            // Increment counters and update metrics
                            _messagesReceivedCounter.Inc();

                            var receivedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                            var producedTimestamp = result.Message.Timestamp.UnixTimestampMs;

                            var latency = receivedTimestamp - producedTimestamp;

                            _logger.LogInformation($"Latency from ASP.NET producer to ASP.NET consumer {latency}ms");

                            _producerToConsumerLatencyHistogram.Observe(latency);

                            using (var scope = _serviceProvider.CreateScope())
                            {
                                var databaseService = scope.ServiceProvider.GetRequiredService<DatabaseService>();

                                using (JsonDocument document = JsonDocument.Parse(result.Message.Value))
                                {
                                    var root = document.RootElement;

                                    var additionalData = new BsonDocument();

                                    foreach (var property in root.EnumerateObject())
                                    {
                                        additionalData.Add(property.Name, BsonValue.Create(property.Value.ToString()));
                                    }

                                    var kafkaEntry = new KafkaEntry
                                    {
                                        CompanyId = result.Topic,
                                        AdditionalData = additionalData
                                    };

                                    await databaseService.SaveDataEntryAsync(kafkaEntry);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka consumer is stopping.");
                }
                finally
                {
                    _consumer.Close();
                }
            }, stoppingToken);
        }
    }
}
