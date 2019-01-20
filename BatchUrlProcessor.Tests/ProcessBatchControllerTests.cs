using BatchUrlProcessor.Controllers;
using BatchUrlProcessor.Helpers;
using BatchUrlProcessor.Models;
using Moq;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BatchUrlProcessor.Tests
{
    public class ProcessBatchControllerTests
    {
        [Fact]
        public async Task UrlReader_Post_Return_ListOfUrlResponses()
        {
            // Arrange
            var mockReader = new Mock<IUrlReader>();
            mockReader.Setup(reader => reader.ReadUrlAsync(It.IsAny<string>()))
                .ReturnsAsync(GetTestResponse());
            var controller = new ProcessBatchController(mockReader.Object);

            // Act
            var result = await controller.Post(new List<string> { "http://google.com", "https://s3.amazonaws.com/uifaces/faces/twitter/marcoramires/128.jpg" });

            // Assert
            var response = Assert.IsType<Response>(result);
            Assert.Equal(2, response.urlResponses.Count);
        }

        [Fact]
        public async Task Url_Reader_Post_Give_Error_Response()
        {
            var mockReader = new Mock<IUrlReader>();
            mockReader.Setup(reader => reader.ReadUrlAsync(It.IsAny<string>()))
                .ReturnsAsync(GetTestErrorResponse());
            var controller = new ProcessBatchController(mockReader.Object);

            var result = await controller.Post(new List<string> { "" });

            var response = Assert.IsType<Response>(result);
            Assert.Single(response.urlResponses);
            Assert.NotNull(response.urlResponses[0].Error);
        }

        [Fact]
        public async Task Url_Reader_Post_Expect_ExceptionAsync()
        {
            var mockReader = new Mock<IUrlReader>();
            mockReader.Setup(reader => reader.ReadUrlAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Failed to get response from url"));
            var controller = new ProcessBatchController(mockReader.Object);

            var result = await controller.Post(new List<string> { "" });

            var response = Assert.IsType<Response>(result);
            Assert.Equal("An unknown error occured when trying to fetch the urls", response.Error.Message);
            Assert.Equal("GE1001", response.Error.Code);
        }

        private UrlResponse GetTestExceptionResponse()
        {
            throw new Exception("Failed to get response from url");
        }

        private UrlResponse GetTestErrorResponse()
        {
            return new UrlResponse() { statusCode = HttpStatusCode.OK, Title = "Sample title", Protocol = Protocol.SSL_and_TLS, Error = new Error() { Code = "HE1001", Message = "Specified url not found" } };
        }

        public UrlResponse GetTestResponse()
        {
            /*return new Response()
            {
                urlResponses = new List<UrlResponse>() {
                    new UrlResponse() { statusCode = HttpStatusCode.OK, Title = "Sample title", TLSStatus = TLSStatus.Both},
                    new UrlResponse() { statusCode = HttpStatusCode.NotFound, Title = "Sample title 2", TLSStatus = TLSStatus.NoTLS},
                }
            };*/
            return new UrlResponse() { statusCode = HttpStatusCode.OK, Title = "Sample title", Protocol = Protocol.SSL_and_TLS };
        }
    }
}
