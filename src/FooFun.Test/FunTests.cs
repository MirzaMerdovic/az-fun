using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FooFun.Test
{
    public class FunTests
    {
        [Fact]
        public async Task Should_Invoke_HttpFunction()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<ILoggerFactory, LoggerFactory>();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var context = new Mock<FunctionContext>();
            context.SetupProperty(c => c.InstanceServices, serviceProvider);
            context.SetupProperty(c => c.Items, new Dictionary<object, object>());

            var fooRequest = new Foo(1, "F o o");
            var fooBytes = JsonSerializer.SerializeToUtf8Bytes(
                fooRequest, 
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            
            var bodyStream = new MemoryStream(fooBytes);

            var request = new Mock<HttpRequestData>(context.Object);
            request.Setup(r => r.Body).Returns(bodyStream);
            request.Setup(r => r.CreateResponse()).Returns(() =>
            {
                var response = new Mock<HttpResponseData>(context.Object);
                response.SetupProperty(r => r.Headers, new HttpHeadersCollection());
                response.SetupProperty(r => r.StatusCode);
                response.SetupProperty(r => r.Body, new MemoryStream());
                return response.Object;
            });

            // Act
            var response = await HttpFunction.Run(request.Object, context.Object);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            response.Body.Seek(0, SeekOrigin.Begin);
            var foo  = await JsonSerializer.DeserializeAsync<Foo>(
                response.Body, 
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            Assert.NotNull(foo);
            Assert.Equal(1, foo.Id);
            Assert.Equal("Processed", foo.Message);
        }
    }
}