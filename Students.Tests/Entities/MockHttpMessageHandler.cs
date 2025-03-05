using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Students.Tests.Entities
{
    /// <summary>
    /// Класс, мокающий метод SendAsync внутри HttpMessageHandler
    /// </summary>
    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public MockHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // Просто возвращаем заготовленное значение
            return await Task.FromResult(_response);
        }
    }
}