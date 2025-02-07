using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Kiosco.Data;
using Kiosco.DTOs;
using Kiosco.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;


namespace Kiosco.Service
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly string _jwtSecret;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _jwtSecret = configuration["Jwt:Secret"]; // Lee la clave secreta desde appsettings.json

            if (string.IsNullOrEmpty(_jwtSecret))
            {
                throw new InvalidOperationException("La clave secreta JWT no está configurada.");
            }

            Console.WriteLine($"Clave secreta cargada: {_jwtSecret}");
        }


        public void MigratePasswords()
        {
            // Obtener todos los usuarios con contraseñas no hasheadas o hasheadas con SHA256
            var users = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.PasswordHash) && !u.PasswordHash.StartsWith("$2")) // Filtra hashes inválidos
                .ToList();

            foreach (var user in users)
            {
                // Re-hashear la contraseña existente con BCrypt
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash, BCrypt.Net.BCrypt.GenerateSalt());
            }

            // Guardar los cambios en la base de datos
            _context.SaveChanges();
        }


        private string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password))
                throw new ArgumentException("La contraseña no puede estar vacía.");

            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public async Task<AuthResponseDto> Register(RegisterDto registerDto)
        {
            
            // Verificar si el email ya está registrado
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == registerDto.Email);
            if (existingUser != null)
            {
                throw new InvalidOperationException("El email ya está registrado.");
            }

            // Hashear la contraseña usando BCrypt
            var passwordHash = HashPassword(registerDto.Password);
            Console.WriteLine($"Hash generado para la contraseña: {passwordHash}");
            // Crear el nuevo usuario
            var newUser = new User
            {
                Email = registerDto.Email,
                PasswordHash = passwordHash, // Aquí se guarda el hash
                Rol = registerDto.Rol,
                RefreshToken = null,
                RefreshTokenExpiry = DateTime.UtcNow
            };

            // Guardar el usuario en la base de datos
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
            Console.WriteLine($"Hash generado para la contraseña: {passwordHash}");

            // Generar tokens JWT para el nuevo usuario
            var accessToken = GenerateAccessToken(newUser);
            var refreshToken = GenerateRefreshToken();

            // Actualizar el Refresh Token del usuario
            newUser.RefreshToken = refreshToken;
            newUser.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            Console.WriteLine($"Hash generado para la contraseña: {passwordHash}");
        }




        public async Task<AuthResponseDto> Authenticate(LoginDto loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !VerifyPassword(loginDto.Password, user.PasswordHash))
            {
                return null; // Credenciales inválidas
            }

            Console.WriteLine("Credenciales válidas. Generando tokens...");
            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            Console.WriteLine($"AccessToken generado: {accessToken}");
            Console.WriteLine($"RefreshToken generado: {refreshToken}");

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<AuthResponseDto> RefreshToken(RefreshTokenDto refreshTokenDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u =>
                !string.IsNullOrEmpty(u.RefreshToken) &&
                u.RefreshTokenExpiry.HasValue &&
                u.RefreshToken == refreshTokenDto.RefreshToken &&
                u.RefreshTokenExpiry.Value > DateTime.UtcNow);

            if (user == null)
            {
                return null; // Refresh Token inválido o expirado
            }

            // Generar un nuevo Access Token
            var accessToken = GenerateAccessToken(user);

            // Generar un nuevo Refresh Token
            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            await _context.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = newRefreshToken
            };
        }

        // Método para autenticar al usuario y generar tokens


        // Método para refrescar el token
       

        // Método privado para generar un Access Token
        private string GenerateAccessToken(User user)
{
    var tokenHandler = new JwtSecurityTokenHandler();
    var key = Encoding.ASCII.GetBytes(_jwtSecret);

    Console.WriteLine($"Generando token con clave secreta: {_jwtSecret}");
    Console.WriteLine($"Claims: Id={user.Id}, Rol={user.Rol}");

    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Role, user.Rol) // Rol del usuario
        }),
        Expires = DateTime.UtcNow.AddMinutes(15), // Expira en 15 minutos
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
        Issuer = "your_issuer",
        Audience = "your_audience"
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var writtenToken = tokenHandler.WriteToken(token);

    Console.WriteLine($"Token generado: {writtenToken}");
    return writtenToken;
}

        // Método privado para generar un Refresh Token
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        // Método privado para verificar la contraseña
        private bool VerifyPassword(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}