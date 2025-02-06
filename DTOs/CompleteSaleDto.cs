using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Kiosco.DTOs
{
    public class CompleteSaleDto
    {
        [Required(ErrorMessage = "El ID del empleado es obligatorio")]
        public int EmpleadoId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El total debe ser mayor que cero")]
        public decimal Total { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public decimal Descuento { get; set; } = 0;

        [Required(ErrorMessage = "Se requieren detalles de la venta")]
        public List<SaleDetailDto> Detalles { get; set; }
    }
}
