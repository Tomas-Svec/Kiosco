using System.ComponentModel.DataAnnotations;

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

        [Required(ErrorMessage = "El rol es obligatorio")]
        public string Rol { get; set; } // Ejemplo: "Empleado", "Jefe"
    }
}
