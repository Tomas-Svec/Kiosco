using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kiosco.Models
{
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        public int VentaId { get; set; } // Clave foránea para la venta

        public int EmpleadoId { get; set; } // Clave foránea para el empleado

        public DateTime FechaModificacion { get; set; } = DateTime.UtcNow;

        public string Accion { get; set; } = string.Empty;

        // Relación con la entidad Sale
        [ForeignKey("VentaId")]
        public Sale Sale { get; set; }
    }
}
