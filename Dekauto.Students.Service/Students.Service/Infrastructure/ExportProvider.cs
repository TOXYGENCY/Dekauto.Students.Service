
﻿using System.Net.Http.Headers;
using System.Text;
﻿using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using System.Configuration;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ExportProvider : IExportProvider
    {
        // используем HttpClientFactory, потому что оно само управляет жизненным циклом соединения и диспозит их + можно мокать
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IStudentsRepository studentsRepository;
        private readonly IStudentsService studentsService;
        private readonly string defaultLatFileName;
        private readonly ILogger<ExportController> logger;

        public ExportProvider(IHttpClientFactory httpClientFactory, IConfiguration configuration,
                IStudentsService studentsService, IStudentsRepository studentsRepository, ILogger<ExportController> logger)
        {
            this.configuration = configuration;
            this.httpClientFactory = httpClientFactory;
            this.studentsRepository = studentsRepository;
            this.studentsService = studentsService;
            defaultLatFileName = configuration["Services:Export:defaultLatFileName"] ?? "exported_student_card";
            this.logger = logger;
        }

        private async Task<ExportFileResult> ExportFile(object data, string apiUrl)
        {
            HttpClient http = httpClientFactory.CreateClient("ExportService");
            // Отправка запроса в сервис "Экспорт" и получение готового файла
            logger.LogInformation("Отправка запроса в сервис \"Экспорт\" и получение готового файла...");
            var response = await http.PostAsJsonAsync(apiUrl, data);
            logger.LogInformation($"Получен ответ с кодом: {response.StatusCode}");
            response.EnsureSuccessStatusCode();

            var fileData = await response.Content.ReadAsByteArrayAsync();
            if (fileData == null) 
            {
                var mes = $"Полученный файл в ответе отсутствует. fileData = {fileData}";
                logger.LogError(mes);
                throw new InvalidDataException(mes);
            }

            var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
                ?? Uri.EscapeDataString($"{defaultLatFileName}");

            logger.LogInformation("Экспорт файла завершен.");
            return new ExportFileResult(fileData, fileName);
        }

        public async Task<ExportFileResult> ExportStudentCardAsync(Guid studentId)
        {
            logger.LogInformation($"Подготовка данных к экспорту студента с id = {studentId}...");
 
            var studentExportDTO = await studentsService.ToExportDtoAsync(studentId);

            if (studentExportDTO == null) throw new KeyNotFoundException(nameof(studentExportDTO));
            // Получаем адрес API из подробного конфига
            var apiUrl = configuration["Services:Export:student_card"];
            if (apiUrl == null) throw new ConfigurationErrorsException(nameof(apiUrl));

            logger.LogInformation($"Подготовка данных завершена.");
            return await ExportFile(studentExportDTO, apiUrl);
        }

        public async Task<ExportFileResult> ExportGroupCardsAsync(Guid groupId)
        {
            logger.LogInformation($"Подготовка данных к экспорту группы с id = {groupId}...");

            var students = await studentsRepository.GetStudentsByGroupAsync(groupId);
            if (!students.Any()) throw new ArgumentException($"Список студентов \"{nameof(students)}\" пуст.");
            var studentExportDTOs = await studentsService.ToExportDtosAsync(students);

            if (!studentExportDTOs.Any()) throw new KeyNotFoundException($"Список студентов \"{nameof(studentExportDTOs)}\" пуст.");
            // Получаем адрес API из подробного конфига
            var apiUrl = configuration["Services:Export:group_cards"];
            if (apiUrl == null) throw new ConfigurationErrorsException(nameof(apiUrl));

            logger.LogInformation($"Подготовка данных завершена.");
            return await ExportFile(studentExportDTOs, apiUrl);
        }
    }
}
