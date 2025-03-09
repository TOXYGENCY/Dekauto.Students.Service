using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Elfie.Serialization;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class StudentsService : IStudentsService
    {
        private readonly IStudentsRepository _studentsRepository;
        private readonly DekautoContext _сontext;

        public StudentsService(IStudentsRepository studentsRepository, DekautoContext сontext)
        {
            _studentsRepository = studentsRepository;
            _сontext = сontext;
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        private DEST _jsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        private async Task<Student> _assignEfStudentModelsAsync(Student student)
        {
            // Только один объект, потому что в нем содержатся нужные id
            // INFO: тут допустимы присвоения null
            student.User = await _сontext.Users.FirstOrDefaultAsync(u => u.Id == student.UserId);
            student.Group = await _сontext.Groups.FirstOrDefaultAsync(gr => gr.Id == student.GroupId);
            student.Oo = await _сontext.Oos.FirstOrDefaultAsync(oo => oo.Id == student.OoId);
            student.AddressResidentialTypeObj = await _сontext.ResidentialTypes.FirstOrDefaultAsync(type => type.Id == student.AddressResidentialTypeId);
            student.AddressRegistrationTypeObj = await _сontext.ResidentialTypes.FirstOrDefaultAsync(type => type.Id == student.AddressRegistrationTypeId);

            return student;
        }

        public async Task<StudentExportDto> ToExportDtoAsync(Guid studentId)
        {
            var student = await _studentsRepository.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentNullException(nameof(student));
            
            // Конвертируем общие поля
            var studentExportDto = _jsonSerializationConvert<Student, StudentExportDto>(student);

            // Дополняем объект нужными данными из БД
            studentExportDto.GroupName = student.Group.Name;
            studentExportDto.OOName = student.Oo.Name;
            studentExportDto.OOAddress = student.Oo.OoAddress;
            studentExportDto.AddressResidentialType = student.AddressResidentialTypeObj.Name;
            studentExportDto.AddressRegistrationType = student.AddressRegistrationTypeObj.Name;
            studentExportDto.EducationReceivedNum = student.EducationReceivedNum;
            studentExportDto.EducationReceivedSerial = student.EducationReceivedSerial;
            studentExportDto.EducationReceivedEndYear = student.EducationReceivedEndYear;

            return studentExportDto;
        }

        public async Task<Student> FromDtoAsync(StudentDto studentDto)
        {
            if (studentDto == null) throw new ArgumentNullException(nameof(studentDto));

            var student = _jsonSerializationConvert<StudentDto, Student>(studentDto);
            student = await _assignEfStudentModelsAsync(student);

            return student;
        }

        // TODO INFO: все методы принимающие id забирают объекты из БД, а не испольуют переданные
        public async Task<IEnumerable<StudentExportDto>> ToExportDtosAsync(IEnumerable<Student> students)
        {
            if (students == null) throw new ArgumentNullException(nameof(students));

            var studentExportDtoList = new List<StudentExportDto>();
            foreach (var student in students)
            {
                studentExportDtoList.Add(await ToExportDtoAsync(student.Id));
            }

            return studentExportDtoList;
        }

        public StudentDto ToDto(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));
            return _jsonSerializationConvert<Student, StudentDto>(student);
        }
        
        public IEnumerable<StudentDto> ToDtos(IEnumerable<Student> students)
        {
            if (students == null) throw new ArgumentNullException(nameof(students));

            var studentDtos = new List<StudentDto>();
            foreach (var student in students)
            {
                studentDtos.Add(_jsonSerializationConvert<Student, StudentDto>(student));
            }

            return studentDtos;
        }

        public async Task UpdateAsync(Guid studentId, StudentDto updatedStudentDto)
        {
            if (updatedStudentDto == null || studentId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedStudentDto.Id != studentId) throw new ArgumentException("ID не совпадают.");

            var student = _jsonSerializationConvert<StudentDto, Student>(updatedStudentDto);
            await _studentsRepository.UpdateAsync(student);

        }

        public async Task AddAsync(StudentDto studentDto)
        {
            var student = await FromDtoAsync(studentDto);
            await _studentsRepository.AddAsync(student);
        }
    }
}
