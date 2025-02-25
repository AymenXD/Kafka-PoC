using Producers.Models;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System.Text.Json;
using Prometheus;
using System.Diagnostics;

namespace Producers.Services
{
    public class KafkaProducerService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger<KafkaProducerService> _logger;

        // Define custom metrics
        private static readonly Counter _messagesSentCounter = Metrics.CreateCounter("kafka_messages_sent_total", "Total number of messages sent to Kafka.");
        private static readonly Gauge _messageSizeGauge = Metrics.CreateGauge("kafka_message_size_bytes", "Size of messages sent to Kafka in bytes.");
        private static readonly Histogram _messageLatencyHistogram = Metrics.CreateHistogram("kafka_message_produce_latency_seconds", "Latency of message production in seconds.");


        public KafkaProducerService(string caCertPath,string certPath, string certPassword, string bootstrapServers, ILogger<KafkaProducerService> logger)
        {

            var config = new ProducerConfig { 
                BootstrapServers = bootstrapServers,
                SecurityProtocol =  SecurityProtocol.Ssl,
                SslCaLocation = caCertPath,
                SslKeystoreLocation = certPath,
                SslKeyPassword = certPassword,
                SslKeystorePassword = certPassword,
                SslEndpointIdentificationAlgorithm = SslEndpointIdentificationAlgorithm.None,
                Debug = "security"
            };
            _producer = new ProducerBuilder<string, string>(config).Build();
            _logger = logger;
        }

        public async Task ProduceAsync (string topic, int partition, object data)
        {
            var message = new Message<string, string>
            {
                Key = partition.ToString(),
                Value = JsonSerializer.Serialize(data),
            };
            try
            {
                var topicParition = new TopicPartition(topic, new Partition(partition));

                // Measure message production latency
                var stopwatch = System.Diagnostics.Stopwatch.StartNew();

                var dr = await _producer.ProduceAsync(topicParition, message);

                stopwatch.Stop();

                // Increment counters and update metrics
                _messagesSentCounter.Inc();
                _messageSizeGauge.Set(System.Text.Encoding.UTF8.GetByteCount(message.Value));
                _messageLatencyHistogram.Observe(stopwatch.Elapsed.TotalSeconds);

                _logger.LogInformation($"Message sent: '{dr.Value}' to '{dr.TopicPartitionOffset}' of {topic}");
            }
            catch (ProduceException<Null, string> e)
            {
                _logger.LogError($"Delivery failed: {e.Error.Reason}");
            }
            _producer.Flush(TimeSpan.FromSeconds(5));
        }
    }
}
