using System;
using System.Collections.Generic;

namespace KafePersonalTid.Models
{
    public class MonthGroup
    {
        public string MonthKey { get; set; }          // T.ex. "2025-05"
        public string MonthName { get; set; }         // T.ex. "Maj 2025"
        public DateTime MonthDate { get; set; }       // Nytt! T.ex. 2025-05-01
        public double TotalHours { get; set; }        // Totalen för månaden
        public List<WeekGroup> Weeks { get; set; }    // Veckor i månaden
    }


    public class WeekGroup
    {
        public DateTime WeekStart { get; set; }       // Måndag
        public double TotalHours { get; set; }        // Totalen för veckan
        public List<WorkEntry> Entries { get; set; }  // Arbetspass i veckan
    }
}
