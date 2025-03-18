using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IConfiguration configuration;
        public ImportController(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        [HttpPost]
        public async Task<ActionResult> ImportFiles([FromForm] ImportFilesAdapter files)
        {
            try
            {
                if (files.ld != null) { using var stream = files.ld.OpenReadStream(); } // Обработка файла ЛД
                if (files.log != null) { using var stream = files.log.OpenReadStream(); }
                if (files.log2 != null) { using var stream = files.log2.OpenReadStream(); }

                // TODO: вызов importProvider = направление на сервис импорта
                return Ok(files);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
