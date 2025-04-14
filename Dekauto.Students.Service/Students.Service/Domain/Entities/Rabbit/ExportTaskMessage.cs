namespace Dekauto.Students.Service.Students.Service.Domain.Entities.Rabbit
{
    public class ExportTaskMessage
    {
        public Guid operationId { get; set; }
        public Guid studentId { get; set; }
    }
}
