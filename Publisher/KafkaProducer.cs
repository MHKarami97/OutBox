using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Publisher.Config;

namespace Publisher;

public class KafkaProducer : IProducer
{
    private readonly IProducer<Null, string> _producer;

    public KafkaProducer(IConfiguration configuration)
    {
        var producerConfig = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"]
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(ProduceConfig config, string message)
    {
        var configure = config as KafkaProducerConfig;

        var kafkaMessage = new Message<Null, string> { Value = message, };

        await _producer.ProduceAsync(configure.Topic, kafkaMessage);
    }
}