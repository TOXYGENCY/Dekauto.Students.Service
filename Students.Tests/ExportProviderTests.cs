using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Domain.Entities.DTO;
using Dekauto.Students.Service.Students.Service.Domain.Interfaces;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;
using Students.Tests.Entities;
using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace Students.Tests
{

    [TestClass]
    public sealed class ExportProviderTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _exportConfigSectionMock;
        private Mock<IStudentsService> _studentsServiceMock;
        private Mock<IStudentsRepository> _studentsRepositoryMock;
        private ExportProvider _exportProvider;
        private string _apiUrlMock;
        private byte[] _expectedData;
        private string _expectedFileName;


        [TestInitialize]
        public void Setup()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _exportConfigSectionMock = new Mock<IConfigurationSection>();
            _studentsServiceMock = new Mock<IStudentsService>();
            _studentsRepositoryMock = new Mock<IStudentsRepository>();
            _configurationMock.Setup(conf => conf.GetSection("Services").GetSection("Export"))
                .Returns(_exportConfigSectionMock.Object);

            _apiUrlMock = "https://student_card_api_mock"; // Обязательно абсолютный адрес
            _expectedData = new byte[] { 1, 2, 3 };
            _expectedFileName = "student_card.mock";
        }

        /// <summary>
        /// Метод установки и подготовки всех моков для теста.
        /// </summary>
        /// <param name="apiUrl">API URL в моке конфига</param>
        /// <param name="responseData">Данные в ответе на http-запрос</param>
        /// <param name="fileName">Имя файла в ответе на http-запрос</param>
        private void _setupMocks(string apiUrl, byte[] responseData, string fileName)
        {
            // Мок API из конфига
            _exportConfigSectionMock.Setup(sect => sect[It.IsAny<string>()])
                .Returns(apiUrl);

            // Мок HTTP-клиента
            var response = new HttpResponseMessage(HttpStatusCode.OK)
                    { Content = new ByteArrayContent(responseData) };

            response.Content.Headers.ContentDisposition =
                    new ContentDispositionHeaderValue("attachment") 
                        { FileNameStar = fileName };

            // Создаем клиент с кастомным классом MockHttpMessageHandler
            var httpClient = new HttpClient(new MockHttpMessageHandler(response));
            // Мокаем БАЗОВЫЙ метод CreateClient(string), а не метод-расширение CreateClient() потому что базовый используется в любом случае, и его можно замокать 
            _httpClientFactoryMock.Setup(fact => fact.CreateClient(It.Is<string>(s => s == string.Empty)))
                .Returns(httpClient);

            _exportProvider = new ExportProvider(_httpClientFactoryMock.Object, _configurationMock.Object, 
                _studentsServiceMock.Object, _studentsRepositoryMock.Object);
        }

        // Экспорт СТУДЕНТА
        [TestMethod]
        public async Task ExportStudentCardAsync_Valid_ReturnsFileDataAndNameSuccess()
        {
            // Arrange
            var studentId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtoAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new StudentExportDto());

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act
            var (byte_arr, fileName) = await _exportProvider.ExportStudentCardAsync(studentId);

            // Assert
            CollectionAssert.AreEqual(_expectedData, byte_arr);
            Assert.AreEqual(_expectedFileName, fileName);
        }

        [TestMethod]
        public async Task ExportStudentCardAsync_NullDto_Exception()
        {
            // Arrange
            var studentId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtoAsync(It.IsAny<Guid>()))
                .ReturnsAsync((StudentExportDto)null);

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _exportProvider.ExportStudentCardAsync(studentId));
        }

        [TestMethod]
        public async Task ExportStudentCardAsync_NullAPI_Exception()
        {
            // Arrange
            var studentId = new Guid();

            _setupMocks(null, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _exportProvider.ExportStudentCardAsync(studentId));
        }



        // Экспорт ГРУППЫ
        [TestMethod]
        public async Task ExportGroupCardsAsync_Valid_ReturnsFileDataAndNameSuccess()
        {
            // Arrange
            var groupId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtosAsync(It.IsAny<List<Student>>()))
                .ReturnsAsync(new List<StudentExportDto> { new StudentExportDto() });
            _studentsRepositoryMock.Setup(rep => rep.GetStudentsByGroupAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Student> { new Student() });

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act
            var (byte_arr, fileName) = await _exportProvider.ExportGroupCardsAsync(groupId);

            // Assert
            CollectionAssert.AreEqual(_expectedData, byte_arr);
            Assert.AreEqual(_expectedFileName, fileName);
        }

        [TestMethod]
        public async Task ExportGroupCardsAsync_EmptyStudents_Exception()
        {
            // Arrange
            var groupId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtosAsync(It.IsAny<List<Student>>()))
                .ReturnsAsync(new List<StudentExportDto> { new StudentExportDto() });
            _studentsRepositoryMock.Setup(rep => rep.GetStudentsByGroupAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Student>());

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _exportProvider.ExportGroupCardsAsync(groupId));
        }

        [TestMethod]
        public async Task ExportGroupCardsAsync_NullStudents_Exception()
        {
            // Arrange
            var groupId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtosAsync(It.IsAny<List<Student>>()))
                .ReturnsAsync(new List<StudentExportDto> { new StudentExportDto() });
            _studentsRepositoryMock.Setup(rep => rep.GetStudentsByGroupAsync(It.IsAny<Guid>()))
                .ReturnsAsync((List<Student>) null);

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _exportProvider.ExportGroupCardsAsync(groupId));
        }

        [TestMethod]
        public async Task ExportGroupCardsAsync_EmptyStudentExportDtos_Exception()
        {
            // Arrange
            var groupId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtosAsync(It.IsAny<List<Student>>()))
                .ReturnsAsync(new List<StudentExportDto>());
            _studentsRepositoryMock.Setup(rep => rep.GetStudentsByGroupAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Student> { new Student() });

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentException>(async () => await _exportProvider.ExportGroupCardsAsync(groupId));
        }

        [TestMethod]
        public async Task ExportGroupCardsAsync_NullStudentExportDtos_Exception()
        {
            // Arrange
            var groupId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtosAsync(It.IsAny<List<Student>>()))
                .ReturnsAsync((List<StudentExportDto>) null);
            _studentsRepositoryMock.Setup(rep => rep.GetStudentsByGroupAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Student> { new Student() });

            _setupMocks(_apiUrlMock, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _exportProvider.ExportGroupCardsAsync(groupId));
        }

        [TestMethod]
        public async Task ExportGroupCardsAsync_NullAPI_Exception()
        {
            // Arrange
            var groupId = new Guid();

            _studentsServiceMock.Setup(serv => serv.ToExportDtosAsync(It.IsAny<List<Student>>()))
                .ReturnsAsync(new List<StudentExportDto> { new StudentExportDto() });
            _studentsRepositoryMock.Setup(rep => rep.GetStudentsByGroupAsync(It.IsAny<Guid>()))
                .ReturnsAsync(new List<Student> { new Student() });

            _setupMocks(null, _expectedData, _expectedFileName);

            // Act & Assert
            await Assert.ThrowsExceptionAsync<ArgumentNullException>(async () => await _exportProvider.ExportGroupCardsAsync(groupId));
        }
    }
}
