using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(HealthStatusService.WebRole.StartupOwin))]

namespace HealthStatusService.WebRole
{
    public partial class StartupOwin
    {
        public void Configuration(IAppBuilder app)
        {
            //AuthStartup.ConfigureAuth(app);
        }
    }
}
