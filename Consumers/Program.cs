using Consumers.Database;
using Consumers.Services;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using Prometheus;

var builder = WebApplication.CreateBuilder(args);

// Load settings from environment variables
var kafkaBrokers = builder.Configuration["Kafka:Brokers"] ?? "kafka-broker-0:9094,kafka-broker-1:9095,kafka-broker-2:9096";
var kafkaConsumerGroup = builder.Configuration["Kafka:Consumer_Group"] ?? "ats-group";
var kafkaCreateTopics = builder.Configuration.GetSection("Kafka:Create_Topics").Get<string[]>() ?? new[] { "ats1-iot-data", "ats2-iot-data", "ats3-iot-data" };
var kafkaSubscribeTopics = builder.Configuration.GetSection("Kafka:Subscribe_Topics").Get<string[]>() ?? new[] { "ats1-iot-data", "ats2-iot-data", "ats3-iot-data" };

var caCertPath = builder.Configuration["Certificate:CAPath"] ?? "Certificates\\TLS_CA.crt";
var certPath = builder.Configuration["Certificate:Path"] ?? "Certificates\\TLS_Client.pfx";
var certPassword = builder.Configuration["Certificate:Password"] ?? "test123";

// Add MongoDB configuration
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration["MongoDB:ConnectionString"] ?? builder.Configuration.GetConnectionString("ConnectionString");
    return new MongoClient(connectionString);
});

builder.Services.AddScoped<DatabaseService>(sp =>
{
    var mongoClient = sp.GetRequiredService<IMongoClient>();
    var logger = sp.GetRequiredService<ILogger<DatabaseService>>();
    return new DatabaseService(mongoClient, logger);
});

builder.Services.AddSingleton<KafkaTopicService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaTopicService>>();
    return new KafkaTopicService(caCertPath, certPath, certPassword, kafkaBrokers, logger, kafkaCreateTopics);
});

builder.Services.AddHostedService<KafkaConsumerService>(sp =>
{
    //var dbService = sp.GetRequiredService<DatabaseService>();
    var serviceProvider = sp.GetRequiredService<IServiceProvider>();
    var logger = sp.GetRequiredService<ILogger<KafkaConsumerService>>();
    return new KafkaConsumerService(caCertPath, certPath, certPassword, logger, kafkaBrokers, kafkaConsumerGroup, kafkaSubscribeTopics, serviceProvider);
});

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// Enable Prometheus metrics
app.UseMetricServer();
app.UseHttpMetrics();

using (var scope = app.Services.CreateScope())
{
    var kafkaTopicService = scope.ServiceProvider.GetRequiredService<KafkaTopicService>();
    await kafkaTopicService.CreateTopicsAsync();
}

app.Run();
