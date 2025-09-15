using System.ComponentModel.DataAnnotations;

namespace Catzy.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Range(0, 9999999999999.99)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [Required, StringLength(100)]
        public string Category { get; set; }

        [StringLength(255)]
        public string Icon { get; set; }
    }
}