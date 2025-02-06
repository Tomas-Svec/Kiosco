using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.DTOs
{
    public class SaleDetailDto
    {
        [Required(ErrorMessage = "El ID de la venta es obligatorio")]
        public int VentaId { get; set; }

        [Required(ErrorMessage = "El ID del producto es obligatorio")]
        public int ProductoId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public int Cantidad { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que cero")]
        public decimal PrecioUnitario { get; set; }
    }
}
