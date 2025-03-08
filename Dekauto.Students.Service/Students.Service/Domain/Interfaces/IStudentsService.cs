using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Entities.ExportAdapters;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsService
    {
        Task<Student> FromDtoAsync(StudentDto studentDTO);

        StudentDto ToDto(Student student);
        IEnumerable<StudentDto> ToDtos(IEnumerable<Student> students);

        Task<StudentExportDto> ToExportDtoAsync(Guid studentId);
        Task<IEnumerable<StudentExportDto>> ToExportDtosAsync(IEnumerable<Student> students);

    }
}
