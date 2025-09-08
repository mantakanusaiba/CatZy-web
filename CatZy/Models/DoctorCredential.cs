using System;

namespace Catzy.Models
{
    public class DoctorCredential
    {
        public int Id { get; set; }
       
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Specialization { get; set; }
        public string ConsultationHours { get; set; }
        public int Experience { get; set; }
        public string Certificates { get; set; }   
        public string ProfilePic { get; set; }     
        public string Status { get; set; }         
    }
}
