using ApiProject.Entities;
using ApiProject.Helpers;
using ApiProject.Models;
using ApiProject.Models.User;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ApiProject.Services
{
    public interface IUserService
    {
        Task<IEnumerable<User>> GetAll();
        Task<User> GetById(int userId);
        Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, string ipAddress);
        Task<AuthenticateResponse> RefreshToken(string refreshToken, string ipAddress);
        Task<bool> RevokeToken(string token, string v);
        Task<User> GetByUsername(string username);
        Task<User> CreateUser(CreateUserRequest request);
    }

    public class UserService : IUserService
    {
        private readonly ApiContext _ctx;
        private readonly AppSettings _appSettings;
        private readonly IMapper _mapper;

        public UserService(ApiContext apiContext, IOptions<AppSettings> appSettings, IMapper mapper)
        {
            _ctx = apiContext;
            _appSettings = appSettings.Value;
            _mapper = mapper;
        }

        public async Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, string ipAddress)
        {
            var user = await _ctx.User.SingleOrDefaultAsync(x => x.Username == request.Username && x.Password == request.Password);

            if (user == null) return null;

            var jwtToken = GenerateJwtToken(user);
            var refreshToken = GenerateRefreshToken(ipAddress);

            user.RefreshTokens.Add(refreshToken);
            _ctx.Update(user);
            _ctx.SaveChanges();

            return new AuthenticateResponse(user, jwtToken, refreshToken.Token);
        }

        public async Task<AuthenticateResponse> RefreshToken(string refreshToken, string ipAddress)
        {
            var user = await _ctx.User.SingleOrDefaultAsync(user => user.RefreshTokens.Any(x => x.Token == refreshToken));

            if (user == null) return null;

            var token = user.RefreshTokens.Single(x => x.Token == refreshToken);

            if (!token.IsActive) return null;

            var newToken = GenerateRefreshToken(ipAddress);
            token.Revoked = DateTime.UtcNow;
            token.RevokedByIp = ipAddress;
            token.ReplacedByToken = newToken.Token;
            user.RefreshTokens.Add(newToken);
            _ctx.Update(user);
            _ctx.SaveChanges();

            var jwtToken = GenerateJwtToken(user);

            return new AuthenticateResponse(user, jwtToken, newToken.Token);
        }

        public async Task<bool> RevokeToken(string token, string ipAddress)
        {
            var user = await _ctx.User.SingleOrDefaultAsync(u => u.RefreshTokens.Any(x => x.Token == token));

            if (user == null) return false;

            var refreshToken = user.RefreshTokens.Single(x => x.Token == token);

            if (!refreshToken.IsActive) return false;

            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _ctx.Update(user);
            _ctx.SaveChanges();

            return true;
        }

        private static RefreshToken GenerateRefreshToken(string ipAddress)
        {
            using var cryptoServiceProvider = RandomNumberGenerator.Create();
            var bytes = new byte[64];
            cryptoServiceProvider.GetBytes(bytes);
            return new RefreshToken()
            {
                Token = Convert.ToBase64String(bytes),
                Expires = DateTime.UtcNow.AddDays(7),
                Created = DateTime.UtcNow,
                CreatedByIp = ipAddress,
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] 
                { 
                    new Claim(ClaimTypes.Name, user.Id.ToString()) 
                }),
                Expires = DateTime.UtcNow.AddMinutes(15),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<IEnumerable<User>> GetAll()
        {
            return await _ctx.User.ToListAsync();
        }

        public async Task<User> GetById(int userId)
        {
            return await _ctx.User.FindAsync(userId);
        }

        public async Task<User> GetByUsername(string username)
        {
            return await _ctx.User.SingleOrDefaultAsync(x => x.Username == username);
        }

        public async Task<User> CreateUser(CreateUserRequest request)
        {
            var user = _mapper.Map<User>(request);

            await _ctx.User.AddAsync(user);
            _ctx.SaveChanges();

            return user;
        }
    }
}
