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
        public IActionResult Login(string returnUrl = "/")
        {
            return Challenge(new AuthenticationProperties() { RedirectUri = returnUrl });
        }

        [HttpGet("user/signout")]
        public async Task<IActionResult> Logout(string returnUrl = "/")
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Redirect($"https://signin.churchofjesuschrist.org/signout?goto={returnUrl}");
            // return Redirect(returnUrl);
        }
    }
}