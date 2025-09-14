using System.ComponentModel.DataAnnotations;

namespace Catzy.Models
{
    public class RescueRequest
    {
        public int Id { get; set; }

        [StringLength(255)]
        public string CatDescription { get; set; }

        [StringLength(255)]
        public string LocationDescription { get; set; }

        [Required]
        public double Latitude { get; set; }

        [Required]
        public double Longitude { get; set; }
    }
}
