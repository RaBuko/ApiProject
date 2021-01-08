using ApiProject.Models;
using ApiProject.Models.User;
using ApiProject.Services;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ApiProject.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUserService _userService;

        public UserController(IUserService userService, IMapper mapper)
        {
            _userService = userService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var user = await _userService.GetById(id);
            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
        {
            var foundUser = _userService.GetByUsername(request.Username);
            if (foundUser != null)
            {
                return BadRequest("Username already taken");
            }

            var user = await _userService.UpsertUser(request);
            return Ok(user);
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest request)
        {
            var response = await _userService.Authenticate(request, GetIPAddress());

            if (response == null)
            {
                return BadRequest(new
                {
                    Message = "Username or password is incorrect"
                });
            }

            SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [AllowAnonymous]
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = await _userService.RefreshToken(refreshToken, GetIPAddress());

            if (response == null)
            {
                return Unauthorized(new 
                { 
                    Message = "Invalid token" 
                });
            }

            SetTokenCookie(response.RefreshToken);

            return Ok(response);
        }        
        
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody]RevokeTokenRequest request)
        {
            string token = request.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
            {
                return BadRequest(new 
                { 
                    Message = "Token is required" 
                });
            }

            var response = await _userService.RevokeToken(token, GetIPAddress());

            if (!response)
            {
                return NotFound(new 
                { 
                    Message = "Token not found" 
                });
            }

            return Ok(new 
            { 
                Message = "Token received" 
            });
        }

        [HttpGet("{id}/refresh-tokens")]
        public async Task<IActionResult> GetRefreshTokens(int id)
        {
            var user = await _userService.GetById(id);
            if (user == null)
            {
                return NotFound();
            }

            return Ok(user.RefreshTokens);
        }

        private void SetTokenCookie(string refreshToken)
        {
            var cookieOption = new CookieOptions()
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddDays(7),
            };
            Response.Cookies.Append("refreshToken", refreshToken, cookieOption);
        }

        private string GetIPAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
