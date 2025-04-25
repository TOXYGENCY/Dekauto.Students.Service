using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/export")]
    [ApiController]
    [Authorize(Policy = "OnlyAdmin")] // Требует аутентификации в роли "Администратор" для всех методов

    public class ExportController : ControllerBase
    {
        private readonly IExportProvider exportProvider;
        private readonly ILogger<ExportController> logger;
        private readonly string defaultLatFileName;

        public ExportController(IExportProvider exportProvider, IConfiguration configuration,
            ILogger<ExportController> logger)
        {
            this.exportProvider = exportProvider;
            this.logger = logger;

            defaultLatFileName = configuration["Services:Export:defaultLatFileName"] ?? "exported_student_card";
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
                logger.LogInformation($"Экспортирована карточка студента с id = {studentId}");

                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Указанный студент не найден.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status404NotFound, mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }

        [HttpPost("group/{groupId}")]
        public async Task<IActionResult> ExportGroupCards(Guid groupId)
        {
            try
            {
                var (fileData, fileName) = await exportProvider.ExportGroupCardsAsync(groupId);
                SetHeaderFileNames(defaultLatFileName, fileName);
                logger.LogInformation($"Экспортирован архив с карточками группы с id = {groupId}");

                return File(fileData, "application/zip");
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Указанный группа не найдена либо она пуста.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status404NotFound, mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }
    }
}
