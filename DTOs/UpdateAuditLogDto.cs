using System.ComponentModel.DataAnnotations;

namespace Kiosco.DTOs
{
    public class UpdateAuditLogDto
    {
        [MaxLength(250, ErrorMessage = "La acción no puede tener más de 250 caracteres")]
        public string Accion { get; set; } // Opcional
    }
}
