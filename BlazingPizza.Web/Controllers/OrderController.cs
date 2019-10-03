using BlazingPizza.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace BlazingPizza.Web.Controllers
{
    [Authorize]
    public class OrderController:Controller
    {
        private OrdersService _ordersService;
        public OrderController(OrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        [HttpPost("orders")]
        public async Task<IActionResult> PlaceOrder([FromBody]Order order)
        {
            var userId = User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value;
            order.UserId = userId;
            var orderId = await _ordersService.SaveOrderAsync(order,userId);
            order.OrderId = orderId;
            return Created($"/myorders/{orderId}", order);
        }

        [HttpGet("myorders")]
        public async Task<IActionResult> OrderStatus()
        {
            var userId = User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value;
            var ordersWithStatus = await _ordersService.GetOrdersAsync(userId);
            return View(ordersWithStatus);
        }

        [HttpGet("myorders/{orderId}")]
        public async Task<IActionResult> OrderDetails(int orderId)
        {
            var userId = User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value;
            var orderWithStatus = await _ordersService.GetOrderWithStatusAsync(orderId, userId);
            return View("OrderDetails", orderWithStatus);
        }

        [HttpGet("/myorders/{orderId}/status")]
        public async Task<IActionResult> OrderStatusUpdate(int orderId)
        {
            var userId = User.Claims.FirstOrDefault(m => m.Type == ClaimTypes.NameIdentifier)?.Value;
            var status = await _ordersService.GetOrderStatusAsync(orderId, userId);
            return Ok(status);
        }
    }
}
