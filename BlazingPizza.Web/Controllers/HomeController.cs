using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using BlazingPizza.Web.Models;
using BlazingPizza.Data;
using Microsoft.AspNetCore.Authentication;

namespace BlazingPizza.Web.Controllers
{
    public class HomeController : Controller
    {
        private SpecialsService _specialsService;
        private ToppingsService _toppingsService;
        public HomeController(SpecialsService specialsService, ToppingsService toppingsService)
        {
            _specialsService = specialsService;
            _toppingsService = toppingsService;
        }
        public async Task<IActionResult> Index()
        {
            var specials = await _specialsService.GetSpecialsAsync();
            return View(specials);
        }

        [HttpGet("configureDialog")]
        public async Task<IActionResult> ConfigureDialogPartial([FromQuery]PizzaSpecial special)
        {
            var configuringPizza = new Pizza()
            {
                Special = special,
                SpecialId = special.Id,
                Size = Pizza.DefaultSize,
                Toppings = new List<PizzaTopping>()
            };

            ViewData["Toppings"] = await _toppingsService.GetToppingsAsync();

            return PartialView("_ConfigurePizzaDialog", configuringPizza);
        }

        [HttpPost("pizza/order")]
        public IActionResult AddPizzaToOrder([FromBody] Order order)
        {
            return PartialView("~/Views/Order/_OrderSideBar.cshtml", order);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    }

}
