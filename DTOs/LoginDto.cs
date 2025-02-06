using System.ComponentModel.DataAnnotations;

namespace Kiosco.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "El email es obligatorio")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        public string Password { get; set; }
    }
}
