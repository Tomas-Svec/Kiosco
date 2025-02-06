using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kiosco.Models
{
    public class SaleDetail
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Sale")]
        public int VentaId { get; set; }
        public Sale Sale { get; set; }

        [ForeignKey("Product")]
        public int ProductoId { get; set; }
        public Product Product { get; set; }

        [Required]
        public int Cantidad { get; set; }

        [Required]
        public decimal PrecioUnitario { get; set; }
    }
}
