using Dekauto.Students.Service.Students.Service.Domain.Entities.Rabbit;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IRabbitMQService
    {
        Task PublishExportTaskAsync(string queueName, ExportTaskMessage message);
    }
}
