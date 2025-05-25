using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ImportProvider : IImportProvider
    {
        private readonly IConfiguration configuration;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IStudentsService studentsService;
        private readonly ILogger<ExportController> logger;
        private readonly IGroupsService groupsService;

        public ImportProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory,
            IStudentsService studentsService, ILogger<ExportController> logger,
            IGroupsService groupsService)
        {
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.studentsService = studentsService;
            this.logger = logger;
            this.groupsService = groupsService;
        }

        private async Task<IEnumerable<StudentExportDto>> SendImportAsync(ImportFilesAdapter files)
        {
            var http = httpClientFactory.CreateClient("ImportService");
            var content = new MultipartFormDataContent();

            if (files.ld != null)
            {
                var fileContent = new StreamContent(files.ld.OpenReadStream());
                content.Add(fileContent, "ld", files.ld.FileName);
            }

            if (files.contract != null)
            {
                var fileContent = new StreamContent(files.contract.OpenReadStream());
                content.Add(fileContent, "contract", files.contract.FileName);
            }

            if (files.journal != null)
            {
                var fileContent = new StreamContent(files.journal.OpenReadStream());
                content.Add(fileContent, "journal", files.journal.FileName);
            }

            var response = await http.PostAsync(configuration["Services:Import:import_students"], content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<StudentExportDto>>();
        }

        public async Task ImportFilesAsync(ImportFilesAdapter files)
        {
            logger.LogInformation("Начата передача импорта файлов...");
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (files.ld == null) throw new ArgumentNullException(nameof(files.ld));
            if (files.contract == null) throw new ArgumentNullException(nameof(files.contract));
            if (files.journal == null) throw new ArgumentNullException(nameof(files.journal));

            // Отправка запроса в сервис "Импорт" и получение готового массива
            logger.LogInformation("Отправка запроса в сервис \"Импорт\" и получение готового массива...");
            var newStudents = await SendImportAsync(files);
            logger.LogInformation("Получен массив с готовыми объектами.");

            // Конвертация и добавление в БД
            logger.LogInformation("Получен ответ с готовыми объектами. Начата конвертация и добавление в БД.");
            var groupNames = GetAllUniqueGroupNames(newStudents);
            var existingStudentsInGroups = await groupsService.GetAllStudentsForGroupsAsync(groupNames);
            await studentsService.ImportStudentsAsync(newStudents, existingStudentsInGroups);

            logger.LogInformation("Все объекты были инпортированы в базу данных. Импорт завершен.");
        }

        private IEnumerable<string> GetAllUniqueGroupNames(IEnumerable<StudentExportDto> students)
        {
            if (students is null)
            {
                throw new ArgumentNullException(nameof(students));
            }

            var groupNames = students
                .Where(s => !s.GroupName.IsNullOrEmpty())
                .Select(s => s.GroupName)
                .Distinct();

            return groupNames;
        }
    }
}
