using ApiProject.Entities;
using ApiProject.Helpers;
using ApiProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ApiProject.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAll();
        Task<User> GetById(int userId);
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest request);
        Task<AuthenticateResponse> RefreshToken(string refreshToken, string ipAddress);
        Task<bool> RevokeToken(string token, string v);
    }

    public class UserService : IUserService
    {
        private readonly ApiContext ctx;
        private readonly AppSettings _appSettings;

        public UserService(ApiContext apiContext, IOptions<AppSettings> appSettings)
        {
            ctx = apiContext;
            _appSettings = appSettings.Value;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest request)
        {
            // use hash + salt to compare passwords
            var user = await ctx.User.SingleOrDefaultAsync(x => x.Username == request.Username && x.Password == request.Password);

            if (user == null) return null;

            var token = GenerateJwtToken(user);

            return new AuthenticateResponse(user, token);
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();

            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Id.ToString()) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            var users = await ctx.User.ToListAsync();
            return users;
        }

        public async Task<User> GetById(int userId)
        {
            var user = await ctx.User.SingleOrDefaultAsync(x => x.Id == userId);
            return user;
        }

        public Task<AuthenticateResponse> RefreshToken(string refreshToken, string ipAddress)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RevokeToken(string token, string v)
        {
            throw new NotImplementedException();
        }
    }
}
