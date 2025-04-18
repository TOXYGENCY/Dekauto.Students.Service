﻿using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
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
        private readonly string defaultLatFileName;

        public ExportController(IExportProvider exportProvider, IConfiguration configuration)
        {
            this.exportProvider = exportProvider;
            this.configuration = configuration;

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
    }
}
