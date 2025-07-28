using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MovieData;
using MovieService_WebRole1.Models;
using ApplicationUser = MovieService_WebRole1.Models.ApplicationUser;

// Alias ambiguous types to resolve conflict
using ApplicationUserManager1 = MovieService_WebRole1.Models.ApplicationUserManager;
using ApplicationSignInManager1 = MovieService_WebRole1.Models.ApplicationSignInManager;
using System;

namespace MovieService_WebRole1.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private UserDataRepository repo = new UserDataRepository();

        private ApplicationUserManager1 _userManager;
        private ApplicationSignInManager1 _signInManager;

        public UserController()
        {
            // Do NOT use HttpContext here!
        }

        public UserController(ApplicationUserManager1 userManager, ApplicationSignInManager1 signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ApplicationUserManager1 UserManager
        {
            get => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager1>();
            private set => _userManager = value;
        }

        public ApplicationSignInManager1 SignInManager
        {
            get => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager1>();
            private set => _signInManager = value;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Index()
        {
            return View(repo.RetrieveAllUsers());
        }

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View(new RegisterViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = new ApplicationUser { UserName = model.Email, Email = model.Email };
            var result = await UserManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                var u = new User(model.Email) // sets RowKey
                {
                    Name = model.Name,
                    Password = model.Password,
                    Email = model.Email,
                    Country = model.Country,
                    City = model.City,
                    Address = model.Address,
                    Gender = model.Gender,
                    ImageUrl = model.ImageUrl
                };
                Console.WriteLine("Above");
                try
                {
                    await repo.AddStudent(u);
                }
                catch (Exception ex)
                {
                    // Log exception or add to model state to show error
                    ModelState.AddModelError("", "Error saving user info: " + ex.Message);
                    return View(model);
                }
                Console.WriteLine("Here");
                return RedirectToAction("Index", "User");
            }
            AddErrors(result);
            return View(model);
        }

        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, isPersistent: false, shouldLockout: false);

            switch (result)
            {
                case SignInStatus.Success:
                    return RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return View("Lockout");
                case SignInStatus.RequiresVerification:
                    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
            return RedirectToAction("Index", "User");
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors) ModelState.AddModelError("", error);
        }
    }
}
