using System.Net;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FooFun
{
    public record Foo
    {
        public Foo(int id, string message)
        {
            Id = id;
            Message = message;
        }

        public int Id { get; init; }

        public string Message { get; init; }
    }

    public static class HttpFunction
    {
        [Function("HttpFunction")]
        public static async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = null)] HttpRequestData req,
            FunctionContext executionContext)
        {
            var logger = executionContext.GetLogger("HttpFunction");
            logger.LogInformation("message processed");

            var rawRequest = await req.ReadAsStringAsync();
            var foo = JsonSerializer.Deserialize<Foo>(rawRequest, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            foo = foo with { Message = "Processed" };
            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json; charset=utf-8");

            response.WriteString(JsonSerializer.Serialize(foo));

            return response;
        }
    }
}
