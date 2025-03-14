using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.AccessControl;

namespace Students.Tests;

[TestClass]
public sealed class StudentsControllerTests
{
    private Mock<IStudentsService> _studentsServiceMock;
    private Mock<IStudentsRepository> _studentsRepositoryMock;
    private StudentsController _studentsController;


    [TestInitialize]
    public void Setup()
    {
        _studentsServiceMock = new Mock<IStudentsService>();
        _studentsRepositoryMock = new Mock<IStudentsRepository>();
    }

    [TestMethod]
    public async Task GetAllStudents_Valid_Ok()
    {
        // Arrange
        _studentsRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Student>());
        _studentsServiceMock.Setup(x => x.ToDtos(It.IsAny<List<Student>>()))
            .Returns(new List<StudentDto>());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetAllStudents();

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(response.Result);
    }

    [TestMethod]
    public async Task GetAllStudents_RepoExcept_Status500()
    {
        // Arrange
        _studentsRepositoryMock.Setup(x => x.GetAllAsync())
            .ThrowsAsync(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetAllStudents();

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetAllStudents_ServExcept_Status500()
    {
        // Arrange
        _studentsRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Student>());
        _studentsServiceMock.Setup(x => x.ToDtos(It.IsAny<List<Student>>()))
            .Throws(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetAllStudents();

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    // GetById

    [TestMethod]
    public async Task GetStudentById_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        _studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Student());
        _studentsServiceMock.Setup(x => x.ToDto(It.IsAny<Student>()))
            .Returns(new StudentDto());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetStudentById(id);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(response.Result);
    }

    [TestMethod]
    public async Task GetStudentById_RepoExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        _studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetStudentById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetStudentById_ServExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        _studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Student());
        _studentsServiceMock.Setup(x => x.ToDto(It.IsAny<Student>()))
            .Throws(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetStudentById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetStudentById_RepoNull_Status404()
    {
        // Arrange
        var id = new Guid();
        _studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Student)null);

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.GetStudentById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status404NotFound, (response.Result as StatusCodeResult).StatusCode);
    }

    // UpdateStudentAsync

    [TestMethod]
    public async Task UpdateStudentAsync_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        var updatedStudentDto = new StudentDto();
        _studentsServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<StudentDto>()));

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.UpdateStudentAsync(id, updatedStudentDto);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task UpdateStudentAsync_ServExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        var updatedStudentDto = new StudentDto();
        _studentsServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<StudentDto>()))
            .ThrowsAsync(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.UpdateStudentAsync(id, updatedStudentDto);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }

    // AddStudent
    [TestMethod]
    public async Task AddStudentAsync_Valid_Ok()
    {
        // Arrange
        var studentDto = new StudentDto();
        _studentsServiceMock.Setup(x => x.AddAsync(It.IsAny<StudentDto>()));

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.AddStudentAsync(studentDto);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task AddStudentAsync_ServExcept_Status500()
    {
        // Arrange
        var studentDto = new StudentDto();
        _studentsServiceMock.Setup(x => x.AddAsync(It.IsAny<StudentDto>()))
            .ThrowsAsync(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.AddStudentAsync(studentDto);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);
    }

    // DeleteStudent

    [TestMethod]
    public async Task DeleteStudent_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        _studentsRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>()));

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.DeleteStudent(id);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task DeleteStudent_RepoExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        _studentsRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        _studentsController = new StudentsController(_studentsRepositoryMock.Object, _studentsServiceMock.Object);

        // Act
        var response = await _studentsController.DeleteStudent(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }
}
