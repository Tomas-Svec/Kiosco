using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Kiosco.DTOs
{
    public class CompleteSaleDto
    {
        public int EmpleadoId { get; set; }
        public decimal Total { get; set; }
        public decimal? Descuento { get; set; }
        public string MedioPago { get; set; }  // Campo opcional
        public List<SaleDetailDto> Detalles { get; set; }
    }
}
