using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Entities.ExportAdapters;
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

        public async Task<StudentExportDTO> ConvertStudent_ToStudentExportDTOAsync(Guid studentId)
        {
            var student = await _studentsRepository.GetByIdAsync(studentId);
            if (student == null) throw new ArgumentNullException(nameof(student));
            
            // Конвертируем общие поля
            var studentExportDTO = _jsonSerializationConvert<Student, StudentExportDTO>(student);

            // Дополняем объект нужными данными из БД
            studentExportDTO.GroupName = student.Group.Name;
            studentExportDTO.OOName = student.Oo.Name;
            studentExportDTO.OOAddress = student.Oo.OoAddress;
            studentExportDTO.AddressResidentialType = student.AddressResidentialTypeObj.Name;
            studentExportDTO.AddressRegistrationType = student.AddressRegistrationTypeObj.Name;
            studentExportDTO.EducationReceivedNum = student.EducationReceivedNum;
            studentExportDTO.EducationReceivedSerial = student.EducationReceivedSerial;
            studentExportDTO.EducationReceivedEndYear = student.EducationReceivedEndYear;

            return studentExportDTO;
        }

        public async Task<Student> ConvertStudentDTO_ToStudentAsync(StudentDTO studentDTO)
        {
            if (studentDTO == null) throw new ArgumentNullException(nameof(studentDTO));

            var student = _jsonSerializationConvert<StudentDTO, Student>(studentDTO);
            student = await _assignEfStudentModelsAsync(student);

            return student;
        }

        // TODO INFO: все методы принимающие id забирают объекты из БД, а не испольуют переданные
        public async Task<IEnumerable<StudentExportDTO>> ConvertStudentsList_ToStudentExportDTOListAsync(IEnumerable<Student> students)
        {
            if (students == null) throw new ArgumentNullException(nameof(students));

            IEnumerable<StudentExportDTO> studentExportDTOList = new List<StudentExportDTO>();
            foreach (var student in students)
            {
                studentExportDTOList = studentExportDTOList.Append(await ConvertStudent_ToStudentExportDTOAsync(student.Id));
            }

            return studentExportDTOList;
        }
    }
}
