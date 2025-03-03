using Dekauto.Students.Service.Students.Service.Domain.Entities;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IExportProvider
    {
        Task<(byte[], string)> ExportStudentCardAsync(Student student);
        //Task<Stream> RequestExportAsync(IEnumerable<Student> students);
    }
}
