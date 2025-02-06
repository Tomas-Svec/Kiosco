using BCrypt.Net;
using Kiosco.Data;
using Kiosco.Models;

namespace Kiosco.Service
{
    public class PasswordMigrationService
    {
        private readonly ApplicationDbContext _context;

        public PasswordMigrationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public void MigratePasswords()
        {
            // Obtener todos los usuarios con contraseñas no hasheadas
            var users = _context.Users
                .Where(u => !string.IsNullOrEmpty(u.PasswordHash) && !u.PasswordHash.StartsWith("$2")) // Filtra hashes inválidos
                .ToList();

            foreach (var user in users)
            {
                // Hashear la contraseña existente
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            }

            // Guardar los cambios en la base de datos
            _context.SaveChanges();
        }
    }
}
