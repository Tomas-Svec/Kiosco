using System.ComponentModel.DataAnnotations;

namespace Kiosco.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Nombre { get; set; }
    }
}
