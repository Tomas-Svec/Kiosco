using System.ComponentModel.DataAnnotations;

namespace Kiosco.Models
{
    public class Supplier
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Nombre { get; set; }

        [MaxLength(150)]
        public string Contacto { get; set; }

        [MaxLength(20)]
        public string Telefono { get; set; }
    }
}
