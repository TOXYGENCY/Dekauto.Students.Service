using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;

namespace Dekauto.Students.Service.Students.Service.Domain.Interfaces
{
    public interface IStudentsService : IDtoConverter<Student, StudentDto>
    {
        // Метод конвертации через сериализацию в/из JSON
        // Здесь необходим для реализации конвертации из IDtoConverter
        DEST JsonSerializationConvert<SRC, DEST>(SRC src);

        // Специальная логика добавления студента
        Task AddAsync(StudentDto studentDto);
        // Специальная логика обновления студента
        Task UpdateAsync(Guid studentId, StudentDto updatedStudentDto);

        // Преобразование из/в Dto-объект специально для сервиса экспорта
        Task<StudentExportDto> ToExportDtoAsync(Guid studentId);
        Task<IEnumerable<StudentExportDto>> ToExportDtosAsync(IEnumerable<Student> students);
        // Импортирование объектов, полученных из сервиса импорта
        Task<IEnumerable<Student>> ImportStudentsAsync(IEnumerable<StudentExportDto> studentExportDtos);
    }
}
