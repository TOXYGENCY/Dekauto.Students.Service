namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IFileStorageService
    {
        Task SaveAsync(Guid operationId, byte[] fileData, string fileName);
        Task<(byte[] FileData, string FileName)> GetFileAsync(Guid operationId);
        Task DeleteAsync(Guid operationId);
    }
}
