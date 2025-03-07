﻿using Dekauto.Students.Service.Students.Service.Domain.Entities;
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
        // TODO: обезопасить все catch - убрать ex.message из вывода

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
                var studentDto = _studentsService.ToDto(student);
                if (studentDto == null) return StatusCode(StatusCodes.Status404NotFound);
                return Ok(studentDto);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
            }
        }

        [HttpPut("{studentId}")]
        public async Task<IActionResult> UpdateStudentAsync(Guid studentId, StudentDto studentDto)
        {
            if (studentId != studentDto.Id) return StatusCode(StatusCodes.Status400BadRequest);
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
        public async Task<ActionResult<Student>> AddStudentAsync(StudentDto studentDto)
        {
            try
            {
                var student = await _studentsService.FromDtoAsync(studentDto);
                await _studentsRepository.AddAsync(student);
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
