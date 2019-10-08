using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingPizza.Data
{
    public class OrdersService
    {
        private readonly PizzaStoreContext _db;
        public OrdersService(PizzaStoreContext db)
        {
            _db = db;
        }

        public async Task<List<OrderWithStatus>> GetOrdersAsync(string userId)
        {
            var orders = await _db.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .OrderByDescending(o => o.CreatedTime)
                .ToListAsync();

            return orders.Select(o => OrderWithStatus.FromOrder(o)).ToList();
        }

        public async Task<OrderWithStatus> GetOrderWithStatusAsync(int orderId, string userId)
        {
            var order = await _db.Orders
                .Where(o => o.OrderId == orderId)
                .Where(o => o.UserId == userId)
                .Include(o => o.DeliveryLocation)
                .Include(o => o.Pizzas).ThenInclude(p => p.Special)
                .Include(o => o.Pizzas).ThenInclude(p => p.Toppings).ThenInclude(t => t.Topping)
                .SingleOrDefaultAsync();

            if(order is null) { return null; }
            return OrderWithStatus.FromOrder(order);
        }

        public async Task<int> SaveOrderAsync(Order order, string userId)
        {
            order.CreatedTime = DateTime.Now;
            if(order.DeliveryLocation is null)
            {
                order.DeliveryLocation = new LatLong(51.5001, -0.1239);
            }

            order.UserId = userId;
            order.DeliveryAddress.UserId = userId;

            // Enforce existence of Pizza.SpecialId and Topping.ToppingId
            // in the database - prevent the submitter from making up
            // new specials and toppings
            foreach (var pizza in order.Pizzas)
            {
                pizza.SpecialId = pizza.Special.Id;
                pizza.Special = null;

                foreach (var topping in pizza.Toppings)
                {
                    topping.ToppingId = topping.Topping.Id;
                    topping.Topping = null;
                }
            }

            _db.Orders.Attach(order);
            await _db.SaveChangesAsync();
            return order.OrderId;
        }

        public async Task<OrderWithStatus> GetOrderStatusAsync(int orderId, string userId)
        {
            var order = await _db.Orders.FirstOrDefaultAsync(m => m.OrderId == orderId && m.UserId==userId);
            if (order is null) { return null; }
            var orderStatus=  OrderWithStatus.FromOrder(order);
            return orderStatus;
        }
    }
}
