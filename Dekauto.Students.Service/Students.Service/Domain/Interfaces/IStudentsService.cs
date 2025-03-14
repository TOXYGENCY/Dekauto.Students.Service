using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsService : IDtoConverter<Student, StudentDto>
    {
        Task AddAsync(StudentDto studentDto);
        Task UpdateAsync(Guid studentId, StudentDto updatedStudentDto);

        Task<StudentExportDto> ToExportDtoAsync(Guid studentId);
        Task<IEnumerable<StudentExportDto>> ToExportDtosAsync(IEnumerable<Student> students);

    }
}
