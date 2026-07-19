using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

public class RabbitMqPublisher
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;

    public RabbitMqPublisher()
    {
        var factory = new ConnectionFactory
        {
            HostName = "localhost",
            Port = 5672
        };

        _connection = factory.CreateConnectionAsync().Result;
        _channel = _connection.CreateChannelAsync().Result;

        _channel.QueueDeclareAsync(
            queue: "embedding-queue",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null).Wait();
    }

    public class ProductEmbeddingMessage
    {
        public Guid ProductId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }

    public async Task PublishAsync(ProductEmbeddingMessage message)
    {
        var json = JsonSerializer.Serialize(message);

        var body = Encoding.UTF8.GetBytes(json);

        await _channel.BasicPublishAsync(
            exchange: "",
            routingKey: "embedding-queue",
            body: body);
    }
}