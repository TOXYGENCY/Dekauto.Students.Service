using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Entities.ExportAdapters;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsService
    {
        Task<StudentExportDTO> ConvertStudent_ToStudentExportDTOAsync(Guid studentId);
        Task<Student> ConvertStudentDTO_ToStudentAsync(StudentDTO studentDTO);
        Task<IEnumerable<StudentExportDTO>> ConvertStudentsList_ToStudentExportDTOListAsync(IEnumerable<Student> students);


    }
}
