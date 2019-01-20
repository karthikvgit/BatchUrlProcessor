using BatchUrlProcessor.Helpers;
using BatchUrlProcessor.Models;
using Common.Utilities.Helpers;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace UrlReaderApi.Tests
{
    public class UrlReaderTests
    {
        [Fact]
        public async Task UrlReader_UrlResponse()
        {
            // Arrange
            var mockReader = new Mock<IHttpClientHelper>();
            mockReader.Setup(helper => helper.GetResponseMessage(It.IsAny<object>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<Dictionary<string, IEnumerable<string>>>(), It.IsAny<Dictionary<string, IEnumerable<string>>>()))
                .Returns(GetHttpResponseMessage());
            mockReader.Setup(helper => helper.GetResponseMessageWithSSL(It.IsAny<object>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<Dictionary<string, IEnumerable<string>>>(), It.IsAny<Dictionary<string, IEnumerable<string>>>()))
                .Returns(GetHttpResponseMessage());
            var reader = new UrlReader(mockReader.Object);

            // Act
            var result = await reader.ReadUrlAsync("http://google.com");

            // Assert
            var response = Assert.IsType<UrlResponse>(result); 
            Assert.Equal(HttpStatusCode.Accepted, response.statusCode);
            Assert.Equal(Protocol.None, response.Protocol); 
        }

        [Fact]
        public async Task UrlReader_UrlResponse_WithTls()
        {
            // Arrange
            var mockReader = new Mock<IHttpClientHelper>();
            mockReader.Setup(helper => helper.GetResponseMessage(It.IsAny<object>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<Dictionary<string, IEnumerable<string>>>(), It.IsAny<Dictionary<string, IEnumerable<string>>>()))
                .Returns(GetHttpResponseMessage());
            mockReader.Setup(helper => helper.GetResponseMessageWithSSL(It.IsAny<object>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<Dictionary<string, IEnumerable<string>>>(), It.IsAny<Dictionary<string, IEnumerable<string>>>()))
                .Returns(GetHttpResponseMessage());
            var reader = new UrlReader(mockReader.Object);

            // Act
            var result = await reader.ReadUrlAsync("https://google.com");

            // Assert
            var response = Assert.IsType<UrlResponse>(result);
            Assert.Equal(HttpStatusCode.Accepted, response.statusCode);
            Assert.Equal(Protocol.SSL_and_TLS, response.Protocol);
        }

        [Fact]
        public async Task UrlReader_UrlResponse_OnlyTls()
        {
            // Arrange
            var mockReader = new Mock<IHttpClientHelper>();
            mockReader.Setup(helper => helper.GetResponseMessage(It.IsAny<object>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<Dictionary<string, IEnumerable<string>>>(), It.IsAny<Dictionary<string, IEnumerable<string>>>()))
                .Returns(GetHttpResponseMessage());
            mockReader.Setup(helper => helper.GetResponseMessageWithSSL(It.IsAny<object>(), It.IsAny<Uri>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<HttpMethod>(), It.IsAny<TimeSpan>(), It.IsAny<Dictionary<string, IEnumerable<string>>>(), It.IsAny<Dictionary<string, IEnumerable<string>>>()))
                .Returns(GetHttpResponseMessage());
            var reader = new UrlReader(mockReader.Object);

            // Act
            var result = await reader.ReadUrlAsync("https://google.com");

            // Assert
            var response = Assert.IsType<UrlResponse>(result);
            Assert.Equal(HttpStatusCode.Accepted, response.statusCode);
            Assert.Equal(Protocol.SSL_and_TLS, response.Protocol);
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessage()
        {
            return new HttpResponseMessage(HttpStatusCode.Accepted);
        }

        private async Task<HttpResponseMessage> GetHttpResponseMessageForSslError()
        {
            return new HttpResponseMessage(HttpStatusCode.InternalServerError);
        }
    }
}
