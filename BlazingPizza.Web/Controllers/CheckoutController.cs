using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using BlazingPizza.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlazingPizza.Web.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private UserService _userService;
        public CheckoutController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("checkout")]
        public IActionResult Checkout()
        {
            return View();
        }

        [HttpPost("checkout/details")]
        public IActionResult CheckoutDetails([FromBody]Order order)
        {
            var orderWithStatus = new OrderWithStatus();
            orderWithStatus.Order = order;
            return PartialView("~/Views/Order/_OrderDetailsBody.cshtml", orderWithStatus);
        }

        [HttpGet("checkout/recentaddress")]
        public IActionResult GetRecentAddress()
        {
            var userId = User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value;
            var deliveryAddress = _userService.GetUserRecentDeliveryAddress(userId);
            return PartialView("~/Views/Checkout/_AddressEditor.cshtml", deliveryAddress);
        }
    }
}