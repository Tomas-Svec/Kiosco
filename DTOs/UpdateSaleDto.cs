using System.ComponentModel.DataAnnotations;

namespace Kiosco.DTOs
{
    public class UpdateSaleDto
    {
        [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo")]
        public decimal? Descuento { get; set; } // Opcional
    }
}
