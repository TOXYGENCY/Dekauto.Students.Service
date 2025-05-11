using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/import")]
    [ApiController]
    [Authorize(Policy = "OnlyAdmin")] // Требует аутентификации в роли "Администратор" для всех методов
    public class ImportController : ControllerBase
    {
        private readonly IImportProvider importProvider;
        private readonly ILogger<ExportController> logger;
        public ImportController(IImportProvider importProvider, ILogger<ExportController> logger)
        {
            this.importProvider = importProvider;
            this.logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> ImportFilesFromFrontendAsync([FromForm] ImportFilesAdapter files)
        {
            try
            {
                if (files is null)
                {
                    throw new ArgumentNullException(nameof(files));
                }

                await importProvider.ImportFilesAsync(files);

                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                var mes = "Некоторые (или все) файлы не передались на сервер. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status400BadRequest, mes);
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
