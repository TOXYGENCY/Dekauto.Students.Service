using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api")]
    [Authorize(Policy = "OnlyAdmin")] // Требует аутентификации в роли "Администратор" для всех методов
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsRepository studentsRepository;
        private readonly IStudentsService studentsService;

        public StudentsController(IStudentsRepository studentsRepository, IStudentsService studentsService)
        {
            this.studentsRepository = studentsRepository;
            this.studentsService = studentsService;
        }
        // TODO: обезопасить все catch - убрать ex.message из вывода (в продакшен)

        [HttpGet("debug")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudentsDebug()
        {
            try
            {
                var students = await studentsRepository.GetAllAsync();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        // INFO: может вернуть пустой список
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents()
        {
            try
            {
                var students = await studentsRepository.GetAllAsync();
                var studentsDto = studentsService.ToDtos(students);
                return Ok(studentsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<StudentDto>> GetStudentById(Guid studentId)
        {
            try
            {
                var student = await studentsRepository.GetByIdAsync(studentId);
                if (student == null) return StatusCode(StatusCodes.Status404NotFound);
                var studentDto = studentsService.ToDto(student);
                return Ok(studentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid studentId, StudentDto updatedStudentDto)
        {
            try
            {
                await studentsService.UpdateAsync(studentId, updatedStudentDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentAsync(StudentDto studentDto)
        {
            try
            {
                await studentsService.AddAsync(studentDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }


        [HttpDelete("{studentId}")]
        public async Task<IActionResult> DeleteStudent(Guid studentId)
        {
            try
            {
                if (studentId == null) return StatusCode(StatusCodes.Status400BadRequest);
                await studentsRepository.DeleteAsync(studentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { ex.Message, ex.StackTrace });
            }
        }
    }
}
