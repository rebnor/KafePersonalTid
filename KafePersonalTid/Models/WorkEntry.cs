using System;
using System.ComponentModel.DataAnnotations;

namespace KafePersonalTid.Models
{
    public class WorkEntry
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Datum")]
        public DateTime Date { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan? StartTime { get; set; }


        [Required]
        [Range(0, 24)]
        [Display(Name = "Timmar")]
        public double HoursWorked { get; set; } // ← Denna måste finnas


        // Koppling till Employee
        public int EmployeeId { get; set; }
        public Employee? Employee { get; set; }

        [Display(Name = "Lön för passet")]
        public double? WageForThisEntry
        {
            get
            {
                if (Employee != null)
                {
                    return Math.Round(HoursWorked * (double)Employee.HourlyWage, 2);
                }
                return null;
            }
        }

    }

}

