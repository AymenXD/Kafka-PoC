using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Producers.Services;
using Prometheus;
using System;

var builder = WebApplication.CreateBuilder(args);

// Load settings from environment variables
var kafkaBrokers = builder.Configuration["Kafka:Brokers"] ?? "kafka-0:9094,kafka-1:9095";
var kafkaCreateTopics = builder.Configuration.GetSection("Kafka:Create_Topics").Get<string[]>() ?? new[] { "ats1-iot-data", "ats2-iot-data", "ats3-iot-data" };
var ATS1api = builder.Configuration["Companies:ATS1_API"] ?? "http://data-generation:5247/data/ats1";
var ATS1topic = builder.Configuration["Companies:ATS1_Topic"] ?? "ats1-iot-data";
int ATS1delay = int.TryParse(builder.Configuration["Companies:ATS1_Delay"], out var parsedDelay) ? parsedDelay : 1000;
var ATS2api = builder.Configuration["Companies:ATS2_API"] ?? "http://data-generation:5247/data/ats2";
var ATS2topic = builder.Configuration["Companies:ATS2_Topic"] ?? "ats2-iot-data";
var ATS2delay = int.TryParse(builder.Configuration["Companies:ATS2_Delay"], out var parsedDelay2) ? parsedDelay2 : 5000;
var ATS3api = builder.Configuration["Companies:ATS3_API"] ?? "http://data-generation:5247/data/ats3";
var ATS3topic = builder.Configuration["Companies:ATS3_Topic"] ?? "ats3-iot-data";
var ATS3delay = int.TryParse(builder.Configuration["Companies:ATS3_Delay"], out var parsedDelay3) ? parsedDelay3 : 10000;

var caCertPath = builder.Configuration["Certificate:CAPath"] ?? "Certificates\\TLS_CA.crt";
var certPath = builder.Configuration["Certificate:Path"] ?? "Certificates\\TLS_Client.pfx";
var certPassword = builder.Configuration["Certificate:Password"] ?? "test123";

builder.Services.AddControllers();

builder.Services.AddSingleton<KafkaTopicService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaTopicService>>();
    return new KafkaTopicService(caCertPath, certPath, certPassword, kafkaBrokers, logger, kafkaCreateTopics);
});

builder.Services.AddSingleton<KafkaProducerService>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<KafkaProducerService>>();
    return new KafkaProducerService(caCertPath ,certPath, certPassword, kafkaBrokers, logger);
});

builder.Services.AddHostedService(sp =>
{
    var kafkaProducerService = sp.GetRequiredService<KafkaProducerService>();
    var logger = sp.GetRequiredService<ILogger<ATS1ProducerBgService>>();
    return new ATS1ProducerBgService(kafkaProducerService, logger, ATS1api, ATS1topic, ATS1delay);
});

builder.Services.AddHostedService(sp =>
{
    var kafkaProducerService = sp.GetRequiredService<KafkaProducerService>();
    var logger = sp.GetRequiredService<ILogger<ATS2ProducerBgService>>();
    return new ATS2ProducerBgService(kafkaProducerService, logger, ATS2api, ATS2topic, ATS2delay);
});

builder.Services.AddHostedService(sp =>
{
    var kafkaProducerService = sp.GetRequiredService<KafkaProducerService>();
    var logger = sp.GetRequiredService<ILogger<ATS3ProducerBgService>>();
    return new ATS3ProducerBgService(kafkaProducerService, logger, ATS3api, ATS3topic, ATS3delay);
});

builder.Services.AddLogging(configure => configure.AddConsole());
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var app = builder.Build();

// Enable Prometheus metrics
app.UseMetricServer();
app.UseHttpMetrics();

// Create Topics
using (var scope = app.Services.CreateScope())
{
    var kafkaTopicService = scope.ServiceProvider.GetRequiredService<KafkaTopicService>();
    await kafkaTopicService.CreateTopicsAsync();
}

app.Run();
