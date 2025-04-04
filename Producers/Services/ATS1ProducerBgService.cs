﻿using Producers.Models;
using Confluent.Kafka;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;


namespace Producers.Services
{
    public class ATS1ProducerBgService : BackgroundService
    {
        private readonly KafkaProducerService _kafkaProducerService;
        private static readonly HttpClient httpClient = new HttpClient();
        private readonly ILogger<ATS1ProducerBgService> _logger;
        private readonly string _url;
        private readonly string _topic;
        private readonly int _delay;

        public ATS1ProducerBgService(KafkaProducerService kafkaProducerService, ILogger<ATS1ProducerBgService> logger, string api, string topic, int delay)
        {
            _kafkaProducerService = kafkaProducerService;
            _logger = logger;
            _url = api;
            _topic = topic;
            _delay = delay;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ATS1ProducerBgService is starting...");
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var response = await httpClient.GetAsync(_url);
                    if (response.IsSuccessStatusCode)
                    {
                        var responseData = await response.Content.ReadAsStringAsync();
                        var data = JsonSerializer.Deserialize<Ats1DataModel>(responseData);
                        if (data == null)
                        {
                            _logger.LogError("Deserialized data is null.");
                            continue;
                        }

                        var deviceIdProperty = data.DeviceId;
         
                        var partition = deviceIdProperty;

                        await _kafkaProducerService.ProduceAsync(_topic, partition, data);
                        _logger.LogInformation($"Data sent to Kafka topic {_topic}.");
                    }
                    else
                    {
                        _logger.LogWarning($"Failed to get data from {_url}. Status code: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while producing data for ATS1.");
                }

                await Task.Delay(_delay, stoppingToken); // Wait for 1 second
            }
        }
    }
}
