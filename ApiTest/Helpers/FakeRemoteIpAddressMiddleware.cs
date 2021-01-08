using Microsoft.AspNetCore.Http;
using System.Net;
using System.Threading.Tasks;

namespace ApiTest.Helpers
{
    public class FakeRemoteIpAddressMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IPAddress fakeIpAddress = IPAddress.Parse("127.168.1.32");

        public FakeRemoteIpAddressMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext httpContext)
        {
            httpContext.Connection.RemoteIpAddress = fakeIpAddress;

            await next(httpContext);
        }
    }
}
