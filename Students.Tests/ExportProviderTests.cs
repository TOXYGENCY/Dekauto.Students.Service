using Dekauto.Students.Service.Students.Service.Domain.Entities;
using Dekauto.Students.Service.Students.Service.Infrastructure;
using Microsoft.Extensions.Configuration;
using Moq;
using Students.Tests.Entities;
using System.Net;
using System.Net.Http.Headers;

namespace Students.Tests
{

    [TestClass]
    public sealed class ExportProviderTests
    {
        private Mock<IHttpClientFactory> _httpClientFactoryMock;
        private Mock<IConfiguration> _configurationMock;
        private Mock<IConfigurationSection> _exportConfigSectionMock;
        private ExportProvider _exportProvider;

        [TestInitialize]
        public void Setup()
        {
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _configurationMock = new Mock<IConfiguration>();
            _exportConfigSectionMock = new Mock<IConfigurationSection>();
            _configurationMock.Setup(conf => conf.GetSection("Services").GetSection("Export"))
                .Returns(_exportConfigSectionMock.Object);
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

            _exportProvider = new ExportProvider(_httpClientFactoryMock.Object, _configurationMock.Object);
        }

        [TestMethod]
        public async Task ExportStudentCardAsync_ValidStudent_ReturnsFileDataAndName()
        {
            // Arrange
            var student = new Student();
            var apiUrlMock = "https://student_card_api_mock"; // Обязательно абсолютный адрес
            var expectedData = new byte[] { 1, 2, 3 };
            var expectedFileName = "student_card.mock";

            _setupMocks(apiUrlMock, expectedData, expectedFileName);

            // Act
            var (byte_arr, fileName) = await _exportProvider.ExportStudentCardAsync(student);

            // Assert
            CollectionAssert.AreEqual(expectedData, byte_arr);
            Assert.AreEqual(expectedFileName, fileName);
        }
    }
}
