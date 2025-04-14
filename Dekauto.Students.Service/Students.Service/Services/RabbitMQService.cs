using System.Text;
using Dekauto.Students.Service.Students.Service.Domain.Entities.Rabbit;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using NuGet.Protocol.Plugins;
using RabbitMQ.Client;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class RabbitMQService:IRabbitMQService
    {
        private readonly RabbitMQ.Client.IConnection connection;
        private readonly IChannel channel;

        public RabbitMQService(IConfiguration configuration)
        {
            var factory = new ConnectionFactory() 
            {
                HostName = configuration["RabbitMQ:Host"],
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"]
            };
            connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
        }

        public async Task PublishExportTaskAsync(string queueName, ExportTaskMessage message)
        {
            await channel.QueueDeclareAsync(queue: queueName, durable: true, exclusive: false);
            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            await channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
        }
    }
}
