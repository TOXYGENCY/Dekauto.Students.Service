using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Infrastructure
{
    public class ImportProvider : IImportProvider
    {
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection importConfig;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IStudentsService studentsService;
        private readonly DekautoContext context;

        public ImportProvider(IConfiguration configuration, IHttpClientFactory httpClientFactory, 
            DekautoContext context, IStudentsService studentsService)
        {
            this.context = context;
            this.httpClientFactory = httpClientFactory;
            this.configuration = configuration;
            this.studentsService = studentsService;
            this.importConfig = configuration.GetSection("Services").GetSection("Import");
        }

        private async Task<IEnumerable<StudentExportDto>> SendImportAsync(ImportFilesAdapter files)
        {
            var http = httpClientFactory.CreateClient();
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

            var response = await http.PostAsync(importConfig["import_students"], content);
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<IEnumerable<StudentExportDto>>();
        }

        public async Task ImportFilesAsync(ImportFilesAdapter files)
        {
            if (files == null) throw new ArgumentNullException(nameof(files));
            if (files.ld == null) throw new ArgumentNullException(nameof(files.ld));
            if (files.contract == null) throw new ArgumentNullException(nameof(files.contract));
            if (files.journal == null) throw new ArgumentNullException(nameof(files.journal));

            // Отправка запроса в сервис "Импорт" и получение готового массива
            var newStudents = await SendImportAsync(files);

            // Конвертация и добавление в БД
            var processedNewStudents = await studentsService.ImportStudentsAsync(newStudents);

        }
    }
}
