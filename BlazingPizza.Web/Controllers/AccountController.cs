using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace BlazingPizza.Web.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet("Account/Login")]
        [HttpGet("user/signin")]
        public IActionResult SignIn(string returnUrl = "/")
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpGet("signin")]
        public IActionResult Login(string provider, string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl }, provider);
        }

        [HttpGet("user/signout")]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect(returnUrl);
        }
    }
}