using Microsoft.AspNetCore.Mvc;
using Kiosco.DTOs;
using Kiosco.Service;

namespace Kiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }


        //REGISTRO
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // Esto capturará errores de validación
            }

            try
            {
                var response = await _authService.Register(registerDto);
                return Ok(response);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante el registro: {ex.Message}");
                return BadRequest(new { message = ex.Message });
            }
        }


        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            Console.WriteLine($"Intentando autenticar usuario con email: {loginDto.Email}");

            var result = await _authService.Authenticate(loginDto);

            if (result == null || string.IsNullOrEmpty(result.AccessToken) || string.IsNullOrEmpty(result.RefreshToken))
            {
                Console.WriteLine("Autenticación fallida. Credenciales inválidas.");
                return Unauthorized(new { message = "Credenciales inválidas" });
            }

            Console.WriteLine($"Autenticación exitosa. AccessToken: {result.AccessToken}, RefreshToken: {result.RefreshToken}");
            return Ok(result);
        }

        // POST: api/Auth/refresh-token
        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var result = await _authService.RefreshToken(refreshTokenDto);

            if (result == null || string.IsNullOrEmpty(result.AccessToken) || string.IsNullOrEmpty(result.RefreshToken))
            {
                return Unauthorized(new { message = "Refresh token inválido o expirado" });
            }

            return Ok(result);
        }
    }
}