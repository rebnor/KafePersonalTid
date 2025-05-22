using System;
using System.Collections.Generic;

namespace KafePersonalTid.Models
{
    public class EmployeeDetailsViewModel
    {
        public Employee Employee { get; set; }
        public List<MonthGroup> GroupedByMonthAndWeek { get; set; }

        public decimal HourlyWage => Employee.HourlyWage; // <-- lägg till denna
    }
}
