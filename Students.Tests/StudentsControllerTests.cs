using Dekauto.Students.Service;
using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Students.Tests;

[TestClass]
public sealed class StudentsControllerTests
{
    private Mock<IStudentsService> studentsServiceMock;
    private Mock<IStudentsRepository> studentsRepositoryMock;
    private StudentsController studentsController;


    [TestInitialize]
    public void Setup()
    {
        studentsServiceMock = new Mock<IStudentsService>();
        studentsRepositoryMock = new Mock<IStudentsRepository>();
    }

    [TestMethod]
    public async Task GetAllStudents_Valid_Ok()
    {
        // Arrange
        studentsRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Student>());
        studentsServiceMock.Setup(x => x.ToDtos(It.IsAny<List<Student>>()))
            .Returns(new List<StudentDto>());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetAllStudents();

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(response.Result);
    }

    [TestMethod]
    public async Task GetAllStudents_RepoExcept_Status500()
    {
        // Arrange
        studentsRepositoryMock.Setup(x => x.GetAllAsync())
            .ThrowsAsync(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetAllStudents();

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetAllStudents_ServExcept_Status500()
    {
        // Arrange
        studentsRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Student>());
        studentsServiceMock.Setup(x => x.ToDtos(It.IsAny<List<Student>>()))
            .Throws(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetAllStudents();

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    // GetById

    [TestMethod]
    public async Task GetStudentById_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Student());
        studentsServiceMock.Setup(x => x.ToDto(It.IsAny<Student>()))
            .Returns(new StudentDto());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetStudentById(id);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(response.Result);
    }

    [TestMethod]
    public async Task GetStudentById_RepoExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetStudentById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetStudentById_ServExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Student());
        studentsServiceMock.Setup(x => x.ToDto(It.IsAny<Student>()))
            .Throws(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetStudentById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetStudentById_RepoNull_Status404()
    {
        // Arrange
        var id = new Guid();
        studentsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Student)null);

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.GetStudentById(id);

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
        studentsServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<StudentDto>()));

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.UpdateStudentAsync(id, updatedStudentDto);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task UpdateStudentAsync_ServExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        var updatedStudentDto = new StudentDto();
        studentsServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<StudentDto>()))
            .ThrowsAsync(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.UpdateStudentAsync(id, updatedStudentDto);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }

    // AddStudent
    [TestMethod]
    public async Task AddStudentAsync_Valid_Ok()
    {
        // Arrange
        var studentDto = new StudentDto();
        studentsServiceMock.Setup(x => x.AddAsync(It.IsAny<StudentDto>()));

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.AddStudentAsync(studentDto);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task AddStudentAsync_ServExcept_Status500()
    {
        // Arrange
        var studentDto = new StudentDto();
        studentsServiceMock.Setup(x => x.AddAsync(It.IsAny<StudentDto>()))
            .ThrowsAsync(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.AddStudentAsync(studentDto);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);
    }

    // DeleteStudent

    [TestMethod]
    public async Task DeleteStudent_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        studentsRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>()));

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.DeleteStudent(id);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task DeleteStudent_RepoExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        studentsRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        studentsController = new StudentsController(studentsRepositoryMock.Object, studentsServiceMock.Object);

        // Act
        var response = await studentsController.DeleteStudent(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }
}
