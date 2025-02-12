using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Kiosco.Validators;

namespace Kiosco.Models
{
    [Table("Users")]
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Apellido { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        [ValidUserRole] // Aplica la validación personalizada aquí
        public string Rol { get; set; } = string.Empty;

        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; } = new DateTime(1753, 1, 1);
    }
}