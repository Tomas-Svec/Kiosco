using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kiosco.Models
{
    public class Sale
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public DateTime FechaVenta { get; set; } = DateTime.Now;

        [Required]
        public decimal Total { get; set; }

        // Relación con el usuario (Empleado)
        [ForeignKey("User")]
        public int EmpleadoId { get; set; }
        public User User { get; set; }

        public decimal Descuento { get; set; } = 0;

        // Nueva propiedad MedioPago
        [Required]
        [MaxLength(50)]  // Limitar la longitud del medio de pago
        public string MedioPago { get; set; } = "Efectivo";  // Valor por defecto

        // Relación con los detalles de la venta (SaleDetails)
        public ICollection<SaleDetail> SaleDetails { get; set; }  // Asegúrate de tener esta propiedad
    }
}