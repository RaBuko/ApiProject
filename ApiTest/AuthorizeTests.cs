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
using System.Runtime.CompilerServices;
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
                AllowAutoRedirect = false,               

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
                Username = DbUtilities.DbUsers[0].Username,
                Password = DbUtilities.DbUsers[0].Password
            });

            // Act
            var responseRaw = await _client.PostAsync("/user/authenticate", content);

            // Assert
            var response = await ApplyJwtTokenFromRaw(responseRaw);
            Assert.Equal(DbUtilities.DbUsers[0].FirstName, response.FirstName);
        }

        [Fact]
        public async Task GetUsersAfterAuthentication()
        {
            // Arrange
            var content = JsonContent.Create(new AuthenticateRequest()
            {
                Username = DbUtilities.DbUsers[^1].Username,
                Password = DbUtilities.DbUsers[^1].Password
            });
            var responseRaw = await _client.PostAsync("/user/authenticate", content);
            await ApplyJwtTokenFromRaw(responseRaw);

            // Act
            responseRaw = await _client.GetAsync("/user");

            // Assert
            var responseBody = await responseRaw.Content.ReadAsStringAsync();
            var responseUsers = JsonConvert.DeserializeObject<List<User>>(responseBody);
            Assert.Equal(DbUtilities.DbUsers.Count, responseUsers.Count);
        }

        [Fact]
        public async Task RefreshToken()
        {
            // Arrange
            var content = JsonContent.Create(new AuthenticateRequest()
            {
                Username = DbUtilities.DbUsers[^1].Username,
                Password = DbUtilities.DbUsers[^1].Password,
            });
            var responseRaw = await _client.PostAsync("/user/authenticate", content);
            await ApplyJwtTokenFromRaw(responseRaw);

            // Act
            responseRaw = await _client.PostAsync("/user/refresh-token", null);

            // Assert
            await ApplyJwtTokenFromRaw(responseRaw);
        }

        [Fact]
        public async Task RevokeToken()
        {
            // Arrange
            int id = 1;
            var content = JsonContent.Create(new AuthenticateRequest()
            {
                Username = DbUtilities.DbUsers[id - 1].Username,
                Password = DbUtilities.DbUsers[id - 1].Password
            });

            var responseRaw = await _client.PostAsync("/user/authenticate", content);
            await ApplyJwtTokenFromRaw(responseRaw);

            responseRaw = await _client.GetAsync($"/user/{id}/refresh-tokens");
            var responseTokens = JsonConvert.DeserializeObject<List<RefreshToken>>(await responseRaw.Content.ReadAsStringAsync());
            Assert.Single(responseTokens);
            Assert.True(responseTokens[0].IsActive);

            responseRaw = await _client.PostAsync("/user/refresh-token", null);
            await ApplyJwtTokenFromRaw(responseRaw);

            responseRaw = await _client.GetAsync($"/user/{id}/refresh-tokens");
            responseTokens = JsonConvert.DeserializeObject<List<RefreshToken>>(await responseRaw.Content.ReadAsStringAsync());
            Assert.Equal(2, responseTokens.Count);
            Assert.False(responseTokens[0].IsActive);
            Assert.True(responseTokens[1].IsActive);

            content = JsonContent.Create(new RevokeTokenRequest()
            {
                Token = responseTokens[^1].Token,
            });            
            responseRaw = await _client.PostAsync("/user/revoke-token", content);
            responseRaw.EnsureSuccessStatusCode();

            responseRaw = await _client.GetAsync($"/user/{id}/refresh-tokens");
            responseTokens = JsonConvert.DeserializeObject<List<RefreshToken>>(await responseRaw.Content.ReadAsStringAsync());
            Assert.Equal(2, responseTokens.Count);
            Assert.False(responseTokens[0].IsActive);
            Assert.False(responseTokens[1].IsActive);
        }

        private async Task<AuthenticateResponse> ApplyJwtTokenFromRaw(HttpResponseMessage responseRaw)
        {
            responseRaw.EnsureSuccessStatusCode();
            string responseBody = await responseRaw.Content.ReadAsStringAsync();
            var response = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseBody);
            var authResp = new AuthenticateResponse(int.Parse(response["id"]), response["firstName"], response["lastName"], response["username"], response["jwtToken"], string.Empty);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", authResp.JwtToken);
            return authResp;
        }
    }
}
