using Dekauto.Students.Service.Students.Service.Domain.Entities;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IExportProvider
    {
        Task<ExportFileResult> ExportStudentCardAsync(Guid studentId);
        Task<ExportFileResult> ExportGroupCardsAsync(Guid groupId);
    }
}
