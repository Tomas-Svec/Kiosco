using System.ComponentModel.DataAnnotations;
using Kiosco.Models;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.DTOs
{
    public class SaleDetailDto
    {
        public int Id { get; set; }
        public int VentaId { get; set; }
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
    }
}
