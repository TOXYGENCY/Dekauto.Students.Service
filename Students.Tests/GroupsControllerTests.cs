using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Students.Tests;

[TestClass]
public sealed class GroupsControllerTests
{
    private Mock<IGroupsService> groupsServiceMock;
    private Mock<IGroupsRepository> groupsRepositoryMock;
    private GroupsController groupsController;


    [TestInitialize]
    public void Setup()
    {
        groupsServiceMock = new Mock<IGroupsService>();
        groupsRepositoryMock = new Mock<IGroupsRepository>();
    }

    [TestMethod]
    public async Task GetAllGroups_Valid_Ok()
    {
        // Arrange
        groupsRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Group>());
        groupsServiceMock.Setup(x => x.ToDtos(It.IsAny<List<Group>>()))
            .Returns(new List<GroupDto>());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetAllGroups();

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(response.Result);
    }

    [TestMethod]
    public async Task GetAllGroups_RepoExcept_Status500()
    {
        // Arrange
        groupsRepositoryMock.Setup(x => x.GetAllAsync())
            .ThrowsAsync(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetAllGroups();

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetAllGroups_ServExcept_Status500()
    {
        // Arrange
        groupsRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Group>());
        groupsServiceMock.Setup(x => x.ToDtos(It.IsAny<List<Group>>()))
            .Throws(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetAllGroups();

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    // GetById

    [TestMethod]
    public async Task GetGroupById_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        groupsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Group());
        groupsServiceMock.Setup(x => x.ToDto(It.IsAny<Group>()))
            .Returns(new GroupDto());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetGroupById(id);

        // Assert
        Assert.IsInstanceOfType<OkObjectResult>(response.Result);
    }

    [TestMethod]
    public async Task GetGroupById_RepoExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        groupsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetGroupById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetGroupById_ServExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        groupsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(new Group());
        groupsServiceMock.Setup(x => x.ToDto(It.IsAny<Group>()))
            .Throws(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetGroupById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response.Result as ObjectResult).StatusCode);
    }

    [TestMethod]
    public async Task GetGroupById_RepoNull_Status404()
    {
        // Arrange
        var id = new Guid();
        groupsRepositoryMock.Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Group)null);

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.GetGroupById(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status404NotFound, (response.Result as StatusCodeResult).StatusCode);
    }

    // UpdateGroupAsync

    [TestMethod]
    public async Task UpdateGroupAsync_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        var updatedGroupDto = new GroupDto();
        groupsServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<GroupDto>()));

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.UpdateGroupAsync(id, updatedGroupDto);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task UpdateGroupAsync_ServExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        var updatedGroupDto = new GroupDto();
        groupsServiceMock.Setup(x => x.UpdateAsync(It.IsAny<Guid>(), It.IsAny<GroupDto>()))
            .ThrowsAsync(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.UpdateGroupAsync(id, updatedGroupDto);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }

    // AddGroup
    [TestMethod]
    public async Task AddGroupAsync_Valid_Ok()
    {
        // Arrange
        var groupDto = new GroupDto();
        groupsServiceMock.Setup(x => x.AddAsync(It.IsAny<GroupDto>()));

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.AddGroupAsync(groupDto);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task AddGroupAsync_ServExcept_Status500()
    {
        // Arrange
        var groupDto = new GroupDto();
        groupsServiceMock.Setup(x => x.AddAsync(It.IsAny<GroupDto>()))
            .ThrowsAsync(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.AddGroupAsync(groupDto);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);
    }

    // DeleteGroup

    [TestMethod]
    public async Task DeleteGroup_Valid_Ok()
    {
        // Arrange
        var id = new Guid();
        groupsRepositoryMock.Setup(x => x.DeleteByIdAsync(It.IsAny<Guid>()));

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.DeleteGroup(id);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);
    }

    [TestMethod]
    public async Task DeleteGroup_RepoExcept_Status500()
    {
        // Arrange
        var id = new Guid();
        groupsRepositoryMock.Setup(x => x.DeleteByIdAsync(It.IsAny<Guid>()))
            .ThrowsAsync(new Exception());

        groupsController = new GroupsController(groupsRepositoryMock.Object, groupsServiceMock.Object);

        // Act
        var response = await groupsController.DeleteGroup(id);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }
}

