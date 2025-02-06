using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.DTOs
{
    public class UpdateSaleDetailDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public int? Cantidad { get; set; } // Opcional

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que cero")]
        public decimal? PrecioUnitario { get; set; } // Opcional
    }
}
