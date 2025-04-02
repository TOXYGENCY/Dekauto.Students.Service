using Dekauto.Students.Service.Students.Service.Controllers;
using Dekauto.Students.Service.Students.Service.Domain.Entities.Adapters;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Students.Tests;

[TestClass]
public class ImportControllerTests
{
    private Mock<IImportProvider> importProviderMock;
    private Mock<ImportFilesAdapter> importFilesAdapterMock;
    private ImportController importController;

    [TestInitialize]
    public void Setup()
    {
        importProviderMock = new Mock<IImportProvider>();
        importController = new ImportController(importProviderMock.Object);
    }

    [TestMethod]
    public async Task ImportFilesFromFrontendAsync_Valid_Ok()
    {
        // Arrange
        importFilesAdapterMock = new Mock<ImportFilesAdapter>();

        // Act
        var response = await importController.ImportFilesFromFrontendAsync(importFilesAdapterMock.Object);

        // Assert
        Assert.IsInstanceOfType<OkResult>(response);

    }

    [TestMethod]
    public async Task ImportFilesFromFrontendAsync_ProvExcept_Status500()
    {
        // Arrange
        importFilesAdapterMock = new Mock<ImportFilesAdapter>();
        importProviderMock.Setup(x => x.ImportFilesAsync(It.IsAny<ImportFilesAdapter>()))
            .ThrowsAsync(new Exception());

        // Act
        var response = await importController.ImportFilesFromFrontendAsync(importFilesAdapterMock.Object);

        // Assert
        Assert.AreEqual(StatusCodes.Status500InternalServerError, (response as ObjectResult).StatusCode);

    }
}
