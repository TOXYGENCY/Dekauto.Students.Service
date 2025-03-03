using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;

namespace Dekauto.Students.Service.Students.Service.Controllers
{
    [Route("api/")]
    [ApiController]
    public class StudentsController : ControllerBase
    {
        private readonly DekautoContext _context;
        private readonly IStudentsRepository _studentsRepository;

        public StudentsController(DekautoContext context, IStudentsRepository studentsRepository)
        {
            _context = context;
            _studentsRepository = studentsRepository;
        }

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
        public async Task<IActionResult> UpdateStudentAsync(Guid id, Student student)
        {
            if (id != student.Id) return StatusCode(StatusCodes.Status400BadRequest);

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
        public async Task<ActionResult<Student>> AddStudentAsync(Student student)
        {
            try
            {
                _studentsRepository.AddAsync(student);
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
