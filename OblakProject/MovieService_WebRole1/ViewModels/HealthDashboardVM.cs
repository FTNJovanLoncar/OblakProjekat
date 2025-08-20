using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieService_WebRole1.ViewModels
{
    public class HealthPoint
    {
        public string t { get; set; }   // vreme kao string
        public bool up { get; set; }    // dostupnost
    }

    public class HealthDashboardVM
    {
        public string ServiceName { get; set; }
        public DateTime WindowStartUtc { get; set; }
        public DateTime WindowEndUtc { get; set; }
        public List<HealthPoint> Points { get; set; } = new List<HealthPoint>();
        public int TotalChecks { get; set; }
        public int UpChecks { get; set; }
        public double AvailabilityPercent { get; set; }
        public double UnavailabilityPercent => 100 - AvailabilityPercent;
    }
}