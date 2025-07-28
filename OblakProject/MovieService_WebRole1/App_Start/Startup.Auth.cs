using System;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Microsoft.Owin.Security.Google;
using Owin;
using MovieService_WebRole1.Models;

[assembly: OwinStartup(typeof(MovieService_WebRole1.Startup))]

namespace MovieService_WebRole1
{
    public partial class Startup
    {
        // OWIN startup entry point — must be named "Configuration"
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

        // Your existing auth setup method
        public void ConfigureAuth(IAppBuilder app)
        {
            // Configure the db context, user manager and signin manager to use a single instance per request
            app.CreatePerOwinContext(MovieService_WebRole1.Models.ApplicationDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationSignInManager>(ApplicationSignInManager.Create);

            // Enable cookie authentication
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = DefaultAuthenticationTypes.ApplicationCookie,
                LoginPath = new PathString("/User/Login"),
                Provider = new CookieAuthenticationProvider
                {
                    // Validate security stamp when user logs in (password changes etc)
                    OnValidateIdentity = SecurityStampValidator.OnValidateIdentity<ApplicationUserManager, MovieService_WebRole1.Models.ApplicationUser>(
                        validateInterval: TimeSpan.FromMinutes(30),
                        regenerateIdentity: (manager, user) => user.GenerateUserIdentityAsync(manager))
                }
            });

            app.UseExternalSignInCookie(DefaultAuthenticationTypes.ExternalCookie);

            // Two-factor authentication cookies
            app.UseTwoFactorSignInCookie(DefaultAuthenticationTypes.TwoFactorCookie, TimeSpan.FromMinutes(5));
            app.UseTwoFactorRememberBrowserCookie(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);

            // Uncomment below to enable external login providers (Google, Facebook, etc)
            /*
            app.UseMicrosoftAccountAuthentication(
                clientId: "",
                clientSecret: "");

            app.UseTwitterAuthentication(
               consumerKey: "",
               consumerSecret: "");

            app.UseFacebookAuthentication(
               appId: "",
               appSecret: "");

            app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            {
                ClientId = "",
                ClientSecret = ""
            });
            */
        }
    }
}
