using ApiProject.Helpers;
using ApiProject.Models;
using ApiProject.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Threading.Tasks;

namespace ApiProject.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [Authorize]
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

        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate(AuthenticateRequest request)
        {
            var response = await _userService.Authenticate(request);

            if (response == null)
            {
                return BadRequest(new
                {
                    message = "Username or password is incorrect"
                });
            }

            return Ok(response);
        }
        /*
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var response = await _userService.RefreshToken(refreshToken, GetIPAddress());

            if (response == null)
            {
                return Unauthorized(new { Message = "Invalid token" });
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
                return BadRequest(new { Message = "Token is required" });
            }

            var response = await _userService.RevokeToken(token, GetIPAddress());

            if (!response)
            {
                return NotFound(new { Message = "Token not found" });
            }

            return Ok(new { Message = "Token received" });
        }

        [HttpGet("{id}/refresh-tokens")]
        public IActionResult GetRefreshTokens(int id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            return Ok(user);
        }*/

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
            if (Request.Headers.ContainsKey("X-Forwarder-For"))
                return Request.Headers["X-Forwarded-For"];
            else
                return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        }
    }
}
