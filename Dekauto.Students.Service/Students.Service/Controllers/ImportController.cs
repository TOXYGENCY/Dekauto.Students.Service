using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/import")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly IImportProvider importProvider;
        public ImportController(IImportProvider importProvider)
        {
            this.importProvider = importProvider;
        }

        [HttpPost]
        public async Task<ActionResult> ImportFilesFromFrontendAsync([FromForm] ImportFilesAdapter files)
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
