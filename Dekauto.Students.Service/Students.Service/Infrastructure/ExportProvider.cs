using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ExportProvider : IExportProvider
    {
        // используем HttpClientFactory, потому что оно само управляет жизненным циклом соединения и диспозит их
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _exportConfig;

        public ExportProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;

            // Сразу находим секцию из конфига
            _exportConfig = _configuration.GetSection("Services").GetSection("Export");
        }

        public async Task<(byte[], string)> ExportStudentCardAsync(Student student)
        {
            // Получаем адрес API из подробного конфига
            var apiUrl = _exportConfig.GetValue<string>("student_card");
            if (apiUrl == null ) throw new ArgumentNullException(nameof(apiUrl));

            HttpClient http = _httpClientFactory.CreateClient();
            var response = await http.PostAsJsonAsync(apiUrl, student);
            response.EnsureSuccessStatusCode();

            var fileData = await response.Content.ReadAsByteArrayAsync();
            if (fileData == null) throw new Exception($"Файл отсутствует. fileData = {fileData}");

            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar 
                ?? Uri.EscapeDataString($"{student.Name} {student.Surname} {student.Pathronymic}.xlsx");

            return (fileData, fileName);
        }
    }
}
