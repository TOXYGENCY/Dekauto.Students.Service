using System.Collections.Concurrent;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly ConcurrentDictionary<Guid, (byte[] FileData, string FileName)> storage
        = new();
        public Task DeleteAsync(Guid operationId)
        {
            storage.TryRemove(operationId, out _);
            return Task.CompletedTask;
        }

        public Task<(byte[] FileData, string FileName)> GetFileAsync(Guid operationId)
        {
            if (storage.TryGetValue(operationId, out var file))
                return Task.FromResult(file);

            throw new FileNotFoundException("Файл не найден");
        }

        public Task SaveAsync(Guid operationId, byte[] fileData, string fileName)
        {
            storage[operationId] = (fileData, fileName);
            return Task.CompletedTask;
        }
    }
}
