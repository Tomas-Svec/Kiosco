using System.ComponentModel.DataAnnotations;

namespace Kiosco.DTOs
{
    public class AuditLogDto
    {
        [Required(ErrorMessage = "El ID de la venta es obligatorio")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El ID del empleado es obligatorio")]
        public int EmpleadoId { get; set; }

        [Required(ErrorMessage = "La acción es obligatoria")]
        [MaxLength(250, ErrorMessage = "La acción no puede tener más de 250 caracteres")]
        public string Accion { get; set; }
    }
}
