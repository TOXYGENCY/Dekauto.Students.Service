using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ExportProvider : IExportProvider
    {
        // используем HttpClientFactory, потому что оно само управляет жизненным циклом соединения и диспозит их + можно мокать
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _exportConfig;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IStudentsRepository _studentsRepository;
        private readonly IStudentsService _studentsService;
        private readonly string _defaultLatFileName;

        public ExportProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration,
                IStudentsService studentsService, IStudentsRepository studentsRepository)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
            _studentsRepository = studentsRepository;
            _studentsService = studentsService;

            // Сразу находим секцию из конфига
            _exportConfig = _configuration.GetSection("Services").GetSection("Export");
            _defaultLatFileName = _exportConfig["defaultLatFileName"] ?? "exported_student_card";
        }

        private async Task<(byte[], string)> _exportFile(object data, string apiUrl)
        {
            HttpClient http = _httpClientFactory.CreateClient();
            // Посылаем запрос в сервис Экспорт
            var response = await http.PostAsJsonAsync(apiUrl, data);
            response.EnsureSuccessStatusCode();

            var fileData = await response.Content.ReadAsByteArrayAsync();
            if (fileData == null) throw new Exception($"Файл отсутствует. fileData = {fileData}");

            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? Uri.EscapeDataString($"{_defaultLatFileName}");

            return (fileData, fileName);
        }

        public async Task<(byte[], string)> ExportStudentCardAsync(Guid studentId)
        {
            //var student = _studentsRepository.GetByIdAsync(studentId);

            if (studentId == null) throw new ArgumentNullException(nameof(studentId));
            var studentExportDTO = await _studentsService.ToExportDtoAsync(studentId);

            // Получаем адрес API из подробного конфига
            //var apiUrl = _exportConfig.GetValue<string>("student_card"); - метод-расширение не мокается в тестах
            var apiUrl = _exportConfig["student_card"];
            if (apiUrl == null) throw new ArgumentNullException(nameof(apiUrl));

            return await _exportFile(studentExportDTO, apiUrl);
        }

        public async Task<(byte[], string)> ExportGroupCardsAsync(IEnumerable<Student> students)
        {
            if (students == null) throw new ArgumentNullException(nameof(students));
            if (!students.Any()) throw new ArgumentException($"Список студентов \"{nameof(students)}\" пуст.");
            // Получаем адрес API из подробного конфига
            //var apiUrl = _exportConfig.GetValue<string>("group_cards");
            var apiUrl = _exportConfig["group_cards"];
            if (apiUrl == null) throw new ArgumentNullException(nameof(apiUrl));

            return await _exportFile(students, apiUrl);
        }

        public async Task<(byte[], string)> ExportGroupCardsAsync(Guid groupId)
        {
            if (groupId == null) throw new ArgumentNullException(nameof(groupId));
            var students = await _studentsRepository.GetStudentsByGroupAsync(groupId);
            var studentExportDTOs = await _studentsService.ToExportDtosAsync(students);

            if (studentExportDTOs == null) throw new ArgumentNullException(nameof(studentExportDTOs));
            if (!studentExportDTOs.Any()) throw new ArgumentException($"Список студентов \"{nameof(studentExportDTOs)}\" пуст.");
            // Получаем адрес API из подробного конфига
            var apiUrl = _exportConfig["group_cards"];
            if (apiUrl == null) throw new ArgumentNullException(nameof(apiUrl));

            return await _exportFile(studentExportDTOs, apiUrl);
        }
    }
}
