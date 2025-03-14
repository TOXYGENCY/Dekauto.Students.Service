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
        private readonly IStudentsRepository _studentsRepository;
        private readonly IStudentsService _studentsService;

        public StudentsController(IStudentsRepository studentsRepository, IStudentsService studentsService)
        {
            _studentsRepository = studentsRepository;
            _studentsService = studentsService;
        }
        // TODO: обезопасить все catch - убрать ex.message из вывода (в продакшен)

        [HttpGet("debug")]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudentsDebug()
        {
            try
            {
                var students = await _studentsRepository.GetAllAsync();
                return Ok(students);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        // INFO: может вернуть пустой список
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StudentDto>>> GetAllStudents()
        {
            try
            {
                var students = await _studentsRepository.GetAllAsync();
                var studentsDto = _studentsService.ToDtos(students);
                return Ok(studentsDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<StudentDto>> GetStudentById(Guid studentId)
        {
            try
            {
                var student = await _studentsRepository.GetByIdAsync(studentId);
                if (student == null) return StatusCode(StatusCodes.Status404NotFound);
                var studentDto = _studentsService.ToDto(student);
                return Ok(studentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid studentId, StudentDto updatedStudentDto)
        {
            try
            {
                await _studentsService.UpdateAsync(studentId, updatedStudentDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddStudentAsync(StudentDto studentDto)
        {
            try
            {
                await _studentsService.AddAsync(studentDto);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }


        [HttpDelete("{studentId}")]
        public async Task<IActionResult> DeleteStudent(Guid studentId)
        {
            try
            {
                if (studentId == null) return StatusCode(StatusCodes.Status400BadRequest);
                await _studentsRepository.DeleteAsync(studentId);
                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }
    }
}
