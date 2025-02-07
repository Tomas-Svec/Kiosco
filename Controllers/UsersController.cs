using Microsoft.AspNetCore.Mvc;
using Kiosco.Data;
using Kiosco.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Kiosco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/User?pageNumber=1&pageSize=5
        [HttpGet]
        public IActionResult GetUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 5,
            [FromQuery] string email = null,
            [FromQuery] string rol = null,
            [FromQuery] string orderBy = "Id") // Ordenamiento por defecto
        {
            // Validar que pageNumber y pageSize sean mayores que 0
            if (pageNumber <= 0 || pageSize <= 0)
            {
                return BadRequest(new { message = "pageNumber y pageSize deben ser mayores que 0." });
            }

            // Construir la consulta base
            var query = _context.Users.AsQueryable();

            // Aplicar filtros opcionales
            if (!string.IsNullOrEmpty(email))
            {
                query = query.Where(u => u.Email.Contains(email));
            }

            if (!string.IsNullOrEmpty(rol))
            {
                query = query.Where(u => u.Rol == rol);
            }

            // Aplicar ordenamiento dinámico
            query = orderBy.ToLower() switch
            {
                "email" => query.OrderBy(u => u.Email),
                "rol" => query.OrderBy(u => u.Rol),
                _ => query.OrderBy(u => u.Id) // Por defecto
            };

            // Calcular el número total de registros
            var totalRecords = query.Count();

            // Obtener los registros paginados
            var users = query
                .Skip((pageNumber - 1) * pageSize) // Saltar los registros anteriores
                .Take(pageSize) // Tomar los registros de la página actual
                .ToList();

            // Construir la respuesta
            var response = new
            {
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize,
                Users = users
            };

            return Ok(response);
        }

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            return Ok(user);
        }

        [HttpPost]
        public async Task<IActionResult> PostUser(User user)
        {
            // Validar campos obligatorios
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest(new { message = "El correo electrónico y la contraseña son obligatorios." });
            }

            // Validar la fecha de expiración del token
            if (user.RefreshTokenExpiry < new DateTime(1753, 1, 1))
            {
                return BadRequest(new { message = "La fecha de expiración del token no es válida." });
            }

            // Agregar el usuario a la base de datos
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetUser), new { id = user.Id }, user);
        }

        // PUT: api/User/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, User user)
        {
            if (id != user.Id)
            {
                return BadRequest(new { message = "El ID del usuario no coincide con el ID proporcionado." });
            }

            // Validar campos obligatorios
            if (string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.PasswordHash))
            {
                return BadRequest(new { message = "El correo electrónico y la contraseña son obligatorios." });
            }

            _context.Entry(user).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Users.Any(e => e.Id == id))
                {
                    return NotFound(new { message = "Usuario no encontrado." });
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/User/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                return NotFound(new { message = "Usuario no encontrado." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}