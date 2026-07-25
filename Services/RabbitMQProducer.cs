using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using SportBookingSystem.DTO;

namespace SportBookingSystem.Services
{
    public class RabbitMQProducer
    {
        private const string QueueName = "invoice_email_queue";
        private readonly IConfiguration _config;

        public RabbitMQProducer(IConfiguration config)
        {
            _config = config;
        }

        
        public async Task SendMessageAsync(EmailMessageDTO message)
        {
            var hostName = _config["RabbitMQ:HostName"] ?? "localhost";
            var factory = new ConnectionFactory() { HostName = hostName };
            
            
            using var connection = await factory.CreateConnectionAsync();
            using var channel = await connection.CreateChannelAsync();

            
            await channel.QueueDeclareAsync(queue: QueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

            var jsonString = JsonSerializer.Serialize(message);
            var body = Encoding.UTF8.GetBytes(jsonString);

            
            await channel.BasicPublishAsync(exchange: "", routingKey: QueueName, body: body);
        }
    }
}
