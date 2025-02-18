using System.ComponentModel.DataAnnotations;

namespace Kiosco.DTOs
{
    public class SaleDto
    {
        public int Id { get; set; }
        public DateTime FechaVenta { get; set; }
        public decimal Total { get; set; }
        public int EmpleadoId { get; set; }
        public decimal Descuento { get; set; }
        public string MedioPago { get; set; }
        public List<SaleDetailDto> SaleDetails { get; set; }
    }
}