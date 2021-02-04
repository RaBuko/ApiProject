using ApiProject;
using ApiProject.Entities;
using ApiProject.Helpers;
using ApiProject.Mapper;
using ApiProject.Models.User;
using ApiProject.Services;
using ApiTest.Helpers;
using AutoMapper;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace ApiTest.IntegrationTests
{

    public class UserCrudTests : IClassFixture<ApiFactory<Startup>>
    {
        private readonly IUserService _userService;
        private readonly IMapper _mapper;

        public UserCrudTests()
        {
            var dbOptionsBuilder = new DbContextOptionsBuilder<ApiContext>().UseInMemoryDatabase("InMemoryTestDb");
            var db = new ApiContext(dbOptionsBuilder.Options);
            DbUtilities.InitDbForTests(db);
            _mapper = new MapperConfiguration(c =>
            {
                c.AddProfile<UserMapper>();
            }).CreateMapper();

            var appSettings = new AppSettings()
            {
                Secret = Guid.NewGuid().ToString(),
            };
            _userService = new UserService(db, Options.Create(appSettings), _mapper);
        }

        [Fact]
        public async Task CreateUserTest()
        {
            // Arrange
            var user = new User()
            {
                Username = "newuser",
                FirstName = "firstname",
                LastName = "lastname",
                Password = "Password1234!!@",
                Role = Role.Manager,
            };

            var userDb = await _userService.GetByUsername(user.Username);
            Assert.Null(userDb);
            var request = _mapper.Map<CreateUserRequest>(user);

            // Act
            var userResp = await _userService.UpsertUser(request);

            // Assert
            userDb = await _userService.GetByUsername(user.Username);
            Assert.NotNull(userDb);
            Assert.Equal(user.FirstName, userDb.FirstName);
            Assert.Equal(user.FirstName, userResp.FirstName);
        }

        [Fact]
        public async Task UpdateUserTest()
        {
            var user = new User()
            {
                Username = "newuser2",
                FirstName = "firstname2",
                LastName = "lastname2",
                Password = "Password1234!!@2",
                Role = Role.Manager,
            };

            var userDb = await _userService.GetByUsername(user.Username);
            Assert.Null(userDb);
            var request = _mapper.Map<CreateUserRequest>(user);

            // Act
            var userResp = await _userService.UpsertUser(request);
            user.FirstName = "aaaa";
            request.Firstname = user.FirstName;
            userResp = await _userService.UpsertUser(request);

            // Assert
            userDb = await _userService.GetByUsername(user.Username);
            Assert.NotNull(userDb);
            Assert.Equal(user.FirstName, userDb.FirstName);
            Assert.Equal(user.FirstName, userResp.FirstName);
        }
    }
}
