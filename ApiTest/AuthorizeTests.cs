using ApiProject.Entities;
using ApiProject.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Xunit;

namespace ApiTest
{
    public class AuthorizeTests : IClassFixture<ApiFactory<ApiProject.Startup>>
    {
        private readonly HttpClient _client;
        private readonly ApiFactory<ApiProject.Startup> _factory;

        public AuthorizeTests(ApiFactory<ApiProject.Startup> factory)
        {
            // Arrange
            _factory = factory;
            _client = factory.CreateClient(new WebApplicationFactoryClientOptions()
            {
                AllowAutoRedirect = false
            });
        }

        [Fact]
        public async Task FailToAuthenticate()
        {
            // Act
            var response = await _client.GetAsync("/user");

            // Assert
            Assert.True(response.StatusCode == HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Authenticate()
        {
            // Arrange
            var content = JsonContent.Create(new AuthenticateRequest()
            {
                Username = "whyme",
                Password = "ŻŹÓŁĆĘĄ"
            });

            // Act
            var responseRaw = await _client.PostAsync("/user/authenticate", content);

            // Assert
            responseRaw.EnsureSuccessStatusCode();
            string responseBody = await responseRaw.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            Assert.Equal("User2", response["firstName"]);
        }

        [Fact]
        public async Task GetUsersAfterAuthentication()
        {
            // Arrange
            var content = JsonContent.Create(new AuthenticateRequest()
            {
                Username = "testuser",
                Password = "xdxdxd12!"
            });
            var responseRaw = await _client.PostAsync("/user/authenticate", content);
            responseRaw.EnsureSuccessStatusCode();
            string responseBody = await responseRaw.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            var token = response["token"];
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Act
            responseRaw = await _client.GetAsync("/user");

            // Assert
            responseBody = await responseRaw.Content.ReadAsStringAsync();
            var responseUsers = JsonConvert.DeserializeObject<List<User>>(responseBody);
            Assert.Equal(2, responseUsers.Count);
        }
    }
}
