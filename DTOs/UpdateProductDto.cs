using System.ComponentModel.DataAnnotations;

namespace Kiosco.DTOs
{
    public class UpdateProductDto
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string Nombre { get; set; }

        public string Descripcion { get; set; } // Opcional

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio debe ser mayor que cero")]
        public decimal? Precio { get; set; } // Opcional

        [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
        public int? Stock { get; set; } // Opcional

        public int? CategoriaId { get; set; } // Opcional

        public int? ProveedorId { get; set; } // Opcional
    }
}
