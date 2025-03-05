using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ExportProvider : IExportProvider
    {
        // используем HttpClientFactory, потому что оно само управляет жизненным циклом соединения и диспозит их + можно мокать
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _exportConfig;
        private string _defaultLatFileName;

        public ExportProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            // Сразу находим секцию из конфига
            _exportConfig = _configuration.GetSection("Services").GetSection("Export");
            _defaultLatFileName = _exportConfig["defaultLatFileName"] ?? "exported_student_card";
        }

        private async Task<(byte[], string)> _exportFile(object data, string apiUrl)
        {
            HttpClient http = _httpClientFactory.CreateClient();
            var response = await http.PostAsJsonAsync(apiUrl, data);
            response.EnsureSuccessStatusCode();

            var fileData = await response.Content.ReadAsByteArrayAsync();
            if (fileData == null) throw new Exception($"Файл отсутствует. fileData = {fileData}");

            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? Uri.EscapeDataString($"{_defaultLatFileName}");

            return (fileData, fileName);
        }

        public async Task<(byte[], string)> ExportStudentCardAsync(Student student)
        {
            if (student == null) throw new ArgumentNullException(nameof(student));
            // Получаем адрес API из подробного конфига
            //var apiUrl = _exportConfig.GetValue<string>("student_card"); - метод-расширение не мокается в тестах
            var apiUrl = _exportConfig["student_card"];
            if (apiUrl == null) throw new ArgumentNullException(nameof(apiUrl));

            return await _exportFile(student, apiUrl);
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
    }
}
