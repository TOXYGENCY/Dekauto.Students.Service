using System.Text;
using System.Threading.Channels;
using Dekauto.Students.Service.Students.Service.Domain.Entities.Rabbit;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Dekauto.Students.Service.Students.Service.Services.RabbitConsumers
{
    public class ExportStudentConsumer:BackgroundService
    {
        private readonly IServiceScopeFactory scopeFactory;
        private readonly IConnection connection;
        private readonly IChannel channel;

        public ExportStudentConsumer(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory)
        {
            this.scopeFactory = scopeFactory;

            var factory = new ConnectionFactory()
            {
                HostName = configuration["RabbitMQ:Host"],
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"]
            };
            connection = factory.CreateConnectionAsync().GetAwaiter().GetResult();
            channel = connection.CreateChannelAsync().GetAwaiter().GetResult();
            channel.QueueDeclareAsync(queue: "export_student_queue", durable: true, exclusive: false);
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                using (var scope = scopeFactory.CreateAsyncScope()) { 
                    try
                    {
                        var body = ea.Body.ToArray();
                        var message = JsonConvert.DeserializeObject<ExportTaskMessage>(Encoding.UTF8.GetString(body));
                        var exportProvider = scope.ServiceProvider
                            .GetRequiredService<IExportProvider>();
                        var fileStorage = scope.ServiceProvider
                            .GetRequiredService<IFileStorageService>();
                        // Генерируем файл
                        var (fileData, fileName) = await exportProvider.ExportStudentCardAsync(message.studentId);

                        // Сохраняем в хранилище
                        await fileStorage.SaveAsync(message.operationId, fileData, fileName);

                        await channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        // Логирование ошибки
                        await channel.BasicNackAsync(ea.DeliveryTag, false, requeue: false);
                    }
                }
            };

            await channel.BasicConsumeAsync(queue: "export_student_queue", autoAck: false, consumer: consumer);
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
        public override void Dispose()
        {
            channel?.CloseAsync();
            connection?.CloseAsync();
            base.Dispose();
        }
    }
}
