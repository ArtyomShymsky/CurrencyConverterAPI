using CurrencyConverterAPI.Models;
using CurrencyConverterAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace CurrencyConverterAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]    public class AuthController : ControllerBase
    {
        private readonly JwtTokenService _jwtService;

        public AuthController(JwtTokenService jwtService)
        {
            _jwtService = jwtService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (model.Username == "admin" && model.Password == "password")
            {
                var token = _jwtService.GenerateToken(model.Username);
                return Ok(new { token });
            }

            return Unauthorized("Invalid credentials");
        }
    }
}
