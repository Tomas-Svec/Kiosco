using System.ComponentModel.DataAnnotations;
using Kiosco.Validators;

namespace Kiosco.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        [EmailAddress(ErrorMessage = "El formato del email no es válido")]
        public string Email { get; set; }

        public string Nombre { get; set; }
        public string Apellido { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        public string Password { get; set; }

        [Required(ErrorMessage = "El rol es obligatorio.")]
        [ValidUserRole(ErrorMessage = "El rol debe ser 'Empleado' o 'Jefe'.")] // Aplica la validación aquí
        public string Rol { get; set; } = string.Empty;
    }
}
