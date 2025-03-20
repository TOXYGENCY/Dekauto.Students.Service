using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IConfiguration configuration;
        private readonly IImportProvider importProvider;
        public ImportController(IConfiguration configuration, IImportProvider importProvider)
        {
            this.configuration = configuration;
            this.importProvider = importProvider;
        }

        [HttpPost]
        public async Task<ActionResult> ImportFilesFromFrontend([FromForm] ImportFilesAdapter files)
        {
            try
            {
                await importProvider.ImportFilesAsync(files);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
