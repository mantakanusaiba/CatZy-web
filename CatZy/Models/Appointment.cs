using System;

namespace Catzy.Models
{
    public class Appointment
    {
        public int Id { get; set; } 
        public string DoctorName { get; set; }
        public string Specialization { get; set; }
        public string ConsultationHours { get; set; }
        public DateTime Date { get; set; }
        
        public string CatName { get; set; }
        public int Age { get; set; }
        public string Breed { get; set; }
        public string Symptoms { get; set; }
        public string OwnerName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        
    }
}
