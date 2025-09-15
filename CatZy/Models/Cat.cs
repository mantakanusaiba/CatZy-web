using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Catzy.Models
{
    public class Cat
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Breed { get; set; }
        public string Gender { get; set; }
        public string Age { get; set; }
        public string Color { get; set; }
        public string Weight { get; set; }
        public string FavoriteToy { get; set; }
        public string FavoriteTreat { get; set; }
        public string FavoriteActivity { get; set; }
        public string ImageUrl { get; set; }
    }

}
namespace Catzy.Models
{
    public class AdoptionViewModel
    {
        public int CatId { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
    }
}