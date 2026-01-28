namespace Products.Infrastructure.Messaging;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string ProductEventsTopic { get; set; } = "product-events";
    public string GroupId { get; set; } = "products-service";
}

