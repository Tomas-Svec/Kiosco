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

        [ForeignKey("User")]
        public int EmpleadoId { get; set; }
        public User User { get; set; }

        public decimal Descuento { get; set; } = 0;
    }
}
