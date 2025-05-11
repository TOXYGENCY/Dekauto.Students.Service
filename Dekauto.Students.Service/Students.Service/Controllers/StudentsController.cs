using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/students")]
    [Authorize(Policy = "OnlyAdmin")] // Требует аутентификации в роли "Администратор" для всех методов
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentsRepository studentsRepository;
        private readonly IStudentsService studentsService;
        private readonly ILogger<ExportController> logger;

        public StudentsController(IStudentsRepository studentsRepository, IStudentsService studentsService, 
            ILogger<ExportController> logger)
        {
            this.studentsRepository = studentsRepository;
            this.studentsService = studentsService;
            this.logger = logger;
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
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }

        [HttpGet("{studentId}")]
        public async Task<ActionResult<StudentDto>> GetStudentById(Guid studentId)
        {
            try
            {
                var student = await studentsRepository.GetByIdAsync(studentId);
                if (student == null) throw new KeyNotFoundException($"Нет студента с id = {studentId}");
                var studentDto = studentsService.ToDto(student);
                return Ok(studentDto);
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Указанный студент не найден.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status404NotFound, mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid studentId, StudentDto updatedStudentDto)
        {
            try
            {
                if (updatedStudentDto is null)
                {
                    throw new ArgumentNullException(nameof(updatedStudentDto));
                }

                await studentsService.UpdateAsync(studentId, updatedStudentDto);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Указанный студент не найден.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status404NotFound, mes);
            }
            catch (ArgumentNullException ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
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

        [HttpPost]
        public async Task<IActionResult> AddStudentAsync(StudentDto studentDto)
        {
            try
            {
                if (studentDto is null)
                {
                    throw new ArgumentNullException(nameof(studentDto));
                }

                await studentsService.AddAsync(studentDto);
                return Ok();
            }
            catch (ArgumentNullException ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status400BadRequest, mes);
            }
            catch (InvalidOperationException ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже. (Возможно, указанный студент уже существует)";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
            catch (Exception ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
                logger.LogError(ex, mes);
                return StatusCode(StatusCodes.Status500InternalServerError, mes);
            }
        }


        [HttpDelete("{studentId}")]
        public async Task<IActionResult> DeleteStudent(Guid studentId)
        {
            try
            {
                await studentsRepository.DeleteByIdAsync(studentId);
                return Ok();
            }
            catch (KeyNotFoundException ex)
            {
                var mes = "Указанный студент не найден.";
                logger.LogWarning(ex, mes);
                return StatusCode(StatusCodes.Status404NotFound, mes);
            }
            catch (ArgumentNullException ex)
            {
                var mes = "Возникла непредвиденная ошибка сервера. Обратитесь к администратору или попробуйте позже.";
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
