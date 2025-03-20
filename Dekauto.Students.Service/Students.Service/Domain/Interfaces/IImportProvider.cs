using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IImportProvider
    {
        Task ImportFilesAsync(ImportFilesAdapter files);
    }
}
