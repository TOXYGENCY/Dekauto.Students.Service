using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Configuration;
using System.Drawing;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly DekautoContext _context;
        private readonly IExportProvider _exportProvider;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _exportConfig;
        private readonly string _defaultLatFileName;

        public ExportController(IExportProvider exportProvider, IConfiguration configuration, DekautoContext context)
        {
            _exportProvider = exportProvider;
            _configuration = configuration;
            _context = context;

            // Сразу находим секцию из конфига
            _exportConfig = _configuration.GetSection("Services").GetSection("Export");
            _defaultLatFileName = _exportConfig.GetValue<string>("defaultLatFileName") ?? "exported_student_card";
        }

        private void _setHeaderFileNames(string fileName, string fileNameStar)
        {
            // Проблема: передается только сам файл, а его название автомат. вписывается в заголовки, но без поддержки кириллицы.
            // Решение: формируем http-заголовок с поддержкой UTF-8 (для поддержки кириллицы в http-заголовках)
            var encodedFileName = Uri.EscapeDataString(fileNameStar);
            Response.Headers.Append(
                "Content-Disposition",
                $"attachment; filename=\"{fileName}.xlsx\"; filename*=UTF-8''{encodedFileName}"
            );
        }

        [HttpPost("export/student/{studentId}")]
        public async Task<IActionResult> ExportStudentCard(Guid studentId)
        {
            try
            {
                //var student = _context.Students.FirstOrDefault(s => s.Id == studentId);
                //if (student == null) return StatusCode(StatusCodes.Status400BadRequest);

                var (fileData, fileName) = await _exportProvider.ExportStudentCardAsync(studentId);
                _setHeaderFileNames(_defaultLatFileName, fileName);
                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("export/group/{groupId}")]
        public async Task<IActionResult> ExportGroupCards(Guid groupId)
        {
            try
            {
                if (groupId == null) return StatusCode(StatusCodes.Status400BadRequest);

                var (fileData, fileName) = await _exportProvider.ExportGroupCardsAsync(groupId);
                _setHeaderFileNames(_defaultLatFileName, fileName);
                return File(fileData, "application/zip");
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
