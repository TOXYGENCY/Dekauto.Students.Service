using Dekauto.Students.Service.Students.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ExportProvider : IExportProvider
    {
        // используем HttpClientFactory, потому что оно само управляет жизненным циклом соединения и диспозит их + можно мокать
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection exportConfig;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IStudentsRepository studentsRepository;
        private readonly IStudentsService studentsService;
        private readonly string defaultLatFileName;

        public ExportProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration,
                IStudentsService studentsService, IStudentsRepository studentsRepository)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.studentsRepository = studentsRepository;
            this.studentsService = studentsService;

            // Сразу находим секцию из конфига
            exportConfig = this.configuration.GetSection("Services").GetSection("Export");
            defaultLatFileName = exportConfig["defaultLatFileName"] ?? "exported_student_card";
        }

        private async Task<(byte[], string)> ExportFile(object data, string apiUrl)
        {
            HttpClient http = httpClientFactory.CreateClient();
            // Посылаем запрос в сервис Экспорт
            var response = await http.PostAsJsonAsync(apiUrl, data);
            response.EnsureSuccessStatusCode();

            var fileData = await response.Content.ReadAsByteArrayAsync();
            if (fileData == null) throw new Exception($"Файл отсутствует. fileData = {fileData}");

            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? Uri.EscapeDataString($"{defaultLatFileName}");

            return (fileData, fileName);
        }

        public async Task<(byte[], string)> ExportStudentCardAsync(Guid studentId)
        {

            if (studentId == null) throw new ArgumentNullException(nameof(studentId));
            var studentExportDTO = await studentsService.ToExportDtoAsync(studentId);

            if (studentExportDTO == null) throw new ArgumentNullException(nameof(studentExportDTO));
            // Получаем адрес API из подробного конфига
            var apiUrl = exportConfig["student_card"];
            if (apiUrl == null) throw new ArgumentNullException(nameof(apiUrl));

            return await ExportFile(studentExportDTO, apiUrl);
        }

        public async Task<(byte[], string)> ExportGroupCardsAsync(Guid groupId)
        {
            if (groupId == null) throw new ArgumentNullException(nameof(groupId));
            var students = await studentsRepository.GetStudentsByGroupAsync(groupId);
            if (!students.Any()) throw new ArgumentException($"Список студентов \"{nameof(students)}\" пуст.");
            var studentExportDTOs = await studentsService.ToExportDtosAsync(students);

            if (!studentExportDTOs.Any()) throw new ArgumentException($"Список студентов \"{nameof(studentExportDTOs)}\" пуст.");
            // Получаем адрес API из подробного конфига
            var apiUrl = exportConfig["group_cards"];
            if (apiUrl == null) throw new ArgumentNullException(nameof(apiUrl));

            return await ExportFile(studentExportDTOs, apiUrl);
        }
    }
}
