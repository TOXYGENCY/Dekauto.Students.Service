using Dekauto.Students.Service.Students.Service.Domain.Entities.Rabbit;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/export")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly IExportProvider exportProvider;
        private readonly IConfiguration configuration;
        private readonly IConfigurationSection exportConfig;
        private readonly IRabbitMQService rabbitMQService;
        private readonly IFileStorageService fileStorage;
        private readonly string defaultLatFileName;

        public ExportController(IExportProvider exportProvider, IConfiguration configuration, 
            IRabbitMQService rabbitMQService, IFileStorageService fileStorage)
        {
            this.exportProvider = exportProvider;
            this.configuration = configuration;
            this.fileStorage = fileStorage;
            this.rabbitMQService = rabbitMQService;

            // Сразу находим секцию из конфига
            exportConfig = this.configuration.GetSection("Services").GetSection("Export");
            defaultLatFileName = exportConfig.GetValue<string>("defaultLatFileName") ?? "exported_student_card";
        }

        // Проблема: передается только сам файл, а его название автомат. вписывается в заголовки, но без поддержки кириллицы.
        // Решение: формируем http-заголовок с поддержкой UTF-8 (для поддержки кириллицы в http-заголовках)
        private void SetHeaderFileNames(string fileName, string fileNameStar)
        {
            var encodedFileName = Uri.EscapeDataString(fileNameStar);
            Response.Headers.Append(
                "Content-Disposition",
                $"attachment; filename=\"{fileName}.xlsx\"; filename*=UTF-8''{encodedFileName}"
            );
            // Явно разрешаем заголовок Content-Disposition в CORS
            Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition");
        }


        [HttpPost("student/{studentId}")]
        public async Task<IActionResult> ExportStudentCard(Guid studentId)
        {
            try
            {
                var (fileData, fileName) = await exportProvider.ExportStudentCardAsync(studentId);
                SetHeaderFileNames(defaultLatFileName, fileName);

                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpPost("group/{groupId}")]
        public async Task<IActionResult> ExportGroupCards(Guid groupId)
        {
            try
            {
                if (groupId == null) return StatusCode(StatusCodes.Status400BadRequest);

                var (fileData, fileName) = await exportProvider.ExportGroupCardsAsync(groupId);
                SetHeaderFileNames(defaultLatFileName, fileName);

                return File(fileData, "application/zip");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpPost("student/rabbit/{studentId}")]
        public async Task<IActionResult> ExportStudentCardByRabbit(Guid studentId)
        {
            try
            {
                var operationId = Guid.NewGuid();

                // Отправляем задачу в RabbitMQ
                await rabbitMQService.PublishExportTaskAsync(
                    queueName: "export_student_queue",
                    message: new ExportTaskMessage
                    {
                        operationId = operationId,
                        studentId = studentId
                    }
                );

                // Возвращаем клиенту ID операции
                return Accepted(new { operationId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }

        [HttpGet("download/{operationId}")]
        public async Task<IActionResult> DownloadExportedFile(Guid operationId)
        {
            try
            {
                // Получаем файл из хранилища
                var (fileData, fileName) = await fileStorage.GetFileAsync(operationId);

                // Устанавливаем заголовки
                SetHeaderFileNames(defaultLatFileName, fileName);

                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (FileNotFoundException)
            {
                return NotFound("Файл не найден или еще не готов");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { ex.Message });
            }
        }
    }
}
