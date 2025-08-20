using MovieService_WebRole1.DataBase;
using MovieService_WebRole1.ViewModels;
using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace MovieService_WebRole1.Controllers
{
    public class HealthController : Controller
    {
        public async Task<ActionResult> Index(string serviceName = null)
        {
            var endUtc = DateTime.UtcNow;
            var startUtc = endUtc.AddHours(-2);

            using (var db = new DbContextLocal())
            {
                var q = db.HealthChecks.AsNoTracking().Where(h => h.CheckedAt >= startUtc);
                if (!string.IsNullOrWhiteSpace(serviceName))
                    q = q.Where(h => h.ServiceName == serviceName);

                var items = await q.OrderBy(h => h.CheckedAt).ToListAsync();

                var points = items.Select(h => new HealthPoint
                {
                    t = h.CheckedAt.ToLocalTime().ToString("yyyy-MM-ddTHH:mm:ss"),
                    up = h.IsAvailable
                }).ToList();

                var total = items.Count;
                var up = items.Count(h => h.IsAvailable);
                var availability = total == 0 ? 0 : Math.Round(100.0 * up / total, 2);

                var vm = new HealthDashboardVM
                {
                    ServiceName = serviceName,
                    WindowStartUtc = startUtc,
                    WindowEndUtc = endUtc,
                    Points = points,
                    TotalChecks = total,
                    UpChecks = up,
                    AvailabilityPercent = availability
                };

                return View(vm);
            }
        }
    }
}
