using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Drawing;
using System.Text.Json;

namespace Dekauto.Students.Service.Students.Service.Services
{
    public class StudentsService : IStudentsService
    {
        private readonly IStudentsRepository studentsRepository;
        private readonly DekautoContext context;

        public StudentsService(IStudentsRepository studentsRepository, DekautoContext сontext)
        {
            this.studentsRepository = studentsRepository;
            this.context = сontext;
        }

        /// <summary>
        /// Конвертирование из объекта src типа SRC в объект типа DEST через сериализацию и десереализацию в JSON-объект (встроенный авто-маппинг).
        /// </summary>
        /// <typeparam name="SRC"></typeparam>
        /// <typeparam name="DEST"></typeparam>
        /// <param name="src"></param>
        /// <returns></returns>
        public DEST JsonSerializationConvert<SRC, DEST>(SRC src)
        {
            return JsonSerializer.Deserialize<DEST>(JsonSerializer.Serialize(src));
        }

        public async Task<StudentExportDto> ToExportDtoAsync(Guid studentId)
        {
            var student = await studentsRepository.GetByIdAsync(studentId);
            if (student == null) throw new KeyNotFoundException(nameof(student));
            
            // Конвертируем общие поля
            var studentExportDto = JsonSerializationConvert<Student, StudentExportDto>(student);

            // Дополняем объект нужными данными из БД
            if (student.Group == null) 
                throw new InvalidOperationException($"Отсутствует группа у студента {student.Surname} (id = {student.Id})");
            if (student.Oo == null) 
                throw new InvalidOperationException($"Отсутствует образовательная организация у студента {student.Surname} (id = {student.Id})");
            studentExportDto.GroupName = student.Group.Name;
            studentExportDto.OOName = student.Oo.Name;
            studentExportDto.OOAddress = student.Oo.OoAddress;
            studentExportDto.EducationReceivedNum = student.EducationReceivedNum;
            studentExportDto.EducationReceivedSerial = student.EducationReceivedSerial;
            studentExportDto.EducationReceivedEndYear = student.EducationReceivedEndYear;

            return studentExportDto;
        }

        public async Task<Student> FromDtoAsync(StudentDto studentDto)
        {
            if (studentDto == null) throw new ArgumentNullException(nameof(studentDto));

            var student = await context.Students.FirstOrDefaultAsync(s => s.Id == studentDto.Id);
            student ??= JsonSerializationConvert<StudentDto, Student>(studentDto);

            return student;
        }

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
            return JsonSerializationConvert<Student, StudentDto>(student);
        }
        
        public IEnumerable<StudentDto> ToDtos(IEnumerable<Student> students)
        {
            if (students == null) throw new ArgumentNullException(nameof(students));

            var studentDtos = new List<StudentDto>();
            foreach (var student in students)
            {
                studentDtos.Add(JsonSerializationConvert<Student, StudentDto>(student));
            }

            return studentDtos;
        }

        public async Task UpdateAsync(Guid studentId, StudentDto updatedStudentDto)
        {
            if (updatedStudentDto == null || studentId == null) throw new ArgumentNullException("Не все аргументы переданы.");
            if (updatedStudentDto.Id != studentId) throw new ArgumentException("ID не совпадают.");

            var student = JsonSerializationConvert<StudentDto, Student>(updatedStudentDto);
            await studentsRepository.UpdateAsync(student);

        }

        public async Task AddAsync(StudentDto studentDto)
        {
            if (studentDto == null) throw new ArgumentNullException(nameof(studentDto));
            if (await context.Students.FirstOrDefaultAsync(g => g.Id == studentDto.Id) == null)
            {
                var student = await FromDtoAsync(studentDto);
                await studentsRepository.AddAsync(student);
            }
            else
            {
                throw new InvalidOperationException($"Такой элемент уже существует в базе данных; ID = {studentDto.Id}.");
            }
        }

        public async Task<IEnumerable<Student>> ImportStudentsAsync(IEnumerable<StudentExportDto> studentExportDtos)
        {
            if (!studentExportDtos.Any()) throw new ArgumentException($"{nameof(studentExportDtos)} не может быть null или пустым");

            var students = new List<Student>();

            foreach (var studentExportDto in studentExportDtos)
            {
                students.Add(await ImportStudentsFromExportDtosAsync(studentExportDto));
            }

            return students;
        }

        // studentExportDto - без id, но с данными. student - с id, но без данных
        // найти все внешние id и прицепить в id student
        private async Task<Student> ImportStudentsFromExportDtosAsync(StudentExportDto studentExportDto)
        {
            var student = JsonSerializationConvert<StudentExportDto, Student>(studentExportDto);
            // TODO: убрать обработку объектов в свои репозитории

            // Обработка группы
            var group = await context.Groups.FirstOrDefaultAsync(x => x.Name == studentExportDto.GroupName);
            if (group == null)
            {
                group = new Group { Name = studentExportDto.GroupName };
                context.Groups.Add(group);
                // Не сохраняем здесь, но EF отслеживает изменения и сам генерирует Id объектам
            }
            student.GroupId = group.Id;
            student.Group = group;

            // Обработка Oo
            var oo = await context.Oos.FirstOrDefaultAsync(x => x.Name == studentExportDto.OOName);
            if (oo == null)
            {
                oo = new Oo
                {
                    Name = studentExportDto.OOName,
                    OoAddress = studentExportDto.OOAddress
                };
                context.Oos.Add(oo);
                // Не сохраняем здесь, но EF отслеживает изменения
            }
            student.OoId = oo.Id;
            student.Oo = oo;

            context.Students.Add(student);
            await context.SaveChangesAsync(); // Сохраняем все изменения одним запросом

            return student;
        }
    }
}
