using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
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
        private IExportProvider _exportProvider;
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _exportConfig;
        private string _defaultLatFileName;

        public ExportController(IExportProvider exportProvider, IConfiguration configuration)
        {
            _exportProvider = exportProvider;
            _configuration = configuration;

            // Сразу находим секцию из конфига
            _exportConfig = _configuration.GetSection("Services").GetSection("Export");
            _defaultLatFileName = _exportConfig.GetValue<string>("defaultLatFileName") ?? "exported_student_card";
        }

        private void _setHeaderFileNames(string fileName, string fileNameStar)
        {
            // Проблема: передается только сам файл, а его название автомат. вписывается в заголовки, но без поддержки кириллицы.
            // Формируем http-заголовок с поддержкой UTF-8 (для поддержки кириллицы в http-заголовках)
            var encodedFileName = Uri.EscapeDataString(fileNameStar);
            Response.Headers.Append(
                "Content-Disposition",
                $"attachment; filename=\"{fileName}.xlsx\"; filename*=UTF-8''{encodedFileName}"
            );
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportStudentCard(Student student)
        {
            try
            {
                // INFO: в сервисе "Экспорт" теряется информация о всех связанных объектах (group, OO, grade_book и тп)
                // TODO: добавить наименование группы в передаваемый объект студента (сейчас группа является связанным объектом)
                var (fileData, fileName) = await _exportProvider.ExportStudentCardAsync(student);
                _setHeaderFileNames(_defaultLatFileName, fileName);
                return File(fileData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");
            }
            catch (Exception ex) 
            { 
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
