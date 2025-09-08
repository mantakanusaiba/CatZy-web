namespace Catzy.Models
{
    public class RescueRequest
    {
        public int Id { get; set; }
        public string CatDescription { get; set; }
        public string LocationDescription { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
