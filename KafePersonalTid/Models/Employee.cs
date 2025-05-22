using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KafePersonalTid.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Display(Name = "Personnummer")]
        public string PersonalNumber { get; set; }

        [Display(Name = "Telefonnummer")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Adress")]
        public string Address { get; set; }

        [Display(Name = "Anhörig vid akutfall")]
        public string EmergencyContact { get; set; }

        [Display(Name = "Timlön (kr)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyWage { get; set; }


        public List<WorkEntry> WorkEntries { get; set; } = new List<WorkEntry>();
    }
}
