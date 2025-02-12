using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Kiosco.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        public string Descripcion { get; set; }

        [Required]
        public decimal Precio { get; set; }

        [Required]
        public int Stock { get; set; }

        // Relación con Category
        [ForeignKey("Category")]
        public int CategoriaId { get; set; }
        public Category Category { get; set; }
    }
}
