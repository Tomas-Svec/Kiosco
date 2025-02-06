using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Kiosco.DTOs
{
    public class ProductDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal Precio { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int Stock { get; set; }

        [Required(ErrorMessage = "La categoría es obligatoria")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El proveedor es obligatorio")]
        public int ProveedorId { get; set; }
    }
}
