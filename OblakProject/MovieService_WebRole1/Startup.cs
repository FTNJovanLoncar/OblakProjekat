using Microsoft.Owin;
using Owin;

// [assembly: OwinStartup(typeof(MovieService_WebRole1.StartupOwin))]

namespace MovieService_WebRole1
{
    public partial class StartupOwin
    {
        public void Configuration(IAppBuilder app)
        {
            //AuthStartup.ConfigureAuth(app);
        }
    }
}
