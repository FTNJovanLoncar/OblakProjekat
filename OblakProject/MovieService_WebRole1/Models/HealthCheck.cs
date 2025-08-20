using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MovieService_WebRole1.Models
{
    public class HealthCheck
    {
        public long Id { get; set; }
        public string ServiceName { get; set; }
        public DateTime CheckedAt { get; set; }   // preporuka: čuvaj UTC u bazi
        public bool IsAvailable { get; set; }
        public int? LatencyMs { get; set; }
        public string Note { get; set; }
    }
}