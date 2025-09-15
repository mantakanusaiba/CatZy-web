using System;

namespace Catzy.Models
{
    public class CatPost
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Age { get; set; }
        public string Gender { get; set; }
        public string Color { get; set; }
        public string Breed { get; set; }
        public string Description { get; set; }
        public string ImageUrl { get; set; }
        public string PostedBy { get; set; }
        public DateTime PostedAt { get; set; }
        public string Status { get; set; }    // Pending | Approved | Closed
    }
}
