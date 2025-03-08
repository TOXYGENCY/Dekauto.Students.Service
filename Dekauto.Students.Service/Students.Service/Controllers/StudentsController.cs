using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly DekautoContext _context;
        private readonly IStudentsRepository _studentsRepository;
        private readonly IStudentsService _studentsService;

        public StudentsController(DekautoContext context, IStudentsRepository studentsRepository, IStudentsService studentsService)
        {
            _context = context;
            _studentsRepository = studentsRepository;
            _studentsService = studentsService;
        }
        // TODO: обезопасить все catch - убрать ex.message из вывода

        // TODO: преобразовать в DTO
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Student>>> GetAllStudents()
        {
            try
            {
                return Ok(await _studentsRepository.GetAllAsync());
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost("mapped/{studentId}")]
        public async Task<ActionResult<IEnumerable<Student>>> GetMapped(Guid studentId)
        {
            try
            {
                return Ok(await _studentsService.ConvertStudent_ToStudentExportDTOAsync(studentId));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Student>> GetStudentById(Guid id)
        {
            try
            {
                var student = await _studentsRepository.GetByIdAsync(id);
                if (student == null) return StatusCode(StatusCodes.Status404NotFound);
                return Ok(student);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid id, StudentDTO studentDTO)
        {
            if (id != studentDTO.Id) return StatusCode(StatusCodes.Status400BadRequest);
            try
            {
                // TODO: обращение в сервис...
                //return Ok();
                return StatusCode(StatusCodes.Status501NotImplemented);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<ActionResult<Student>> AddStudentAsync(StudentDTO studentDTO)
        {
            try
            {
                var student = await _studentsService.ConvertStudentDTO_ToStudentAsync(studentDTO);
                await _studentsRepository.AddAsync(student);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteStudent(Guid id)
        {
            try
            {
                // TODO: убрать в сервис...
                var student = await _studentsRepository.GetByIdAsync(id);
                if (student == null) return StatusCode(StatusCodes.Status404NotFound);
                _context.Remove(student);
                await _context.SaveChangesAsync();
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
