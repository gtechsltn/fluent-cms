using System.Text.Json;
using Confluent.Kafka;
using FluentCMS.Utils.HookFactory;

namespace FluentCMS.Utils.EventStreaming;

public sealed class KafkaProducer:IProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(string brokerList, ILogger<KafkaProducer> logger)
    {
        var config = new ProducerConfig { BootstrapServers = brokerList };
        _producer = new ProducerBuilder<string, string>(config).Build();
        _logger = logger;
    }
    
    public async Task ProduceRecord(string topic, string operation, EntityMeta meta, Record record)
    {
        var message = new RecordMessage(operation, meta.EntityName, meta.RecordId, record); 
        await _producer.ProduceAsync(topic,
            new Message<string, string> { Key = message.Key, Value = JsonSerializer.Serialize(message) });
        _logger.LogInformation($"Produced Message: topic={topic}, entity={meta.EntityName}, record id={meta.RecordId}");
    }
    
    public void Dispose()
    {
        _producer.Dispose();
    }
}