using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingPizza.Data
{
    public class ToppingsService
    {
        private readonly PizzaStoreContext _db;
        private IMemoryCache _cache;
        public ToppingsService(PizzaStoreContext db, IMemoryCache cache)
        {
            _db = db;
            _cache = cache;
        }

        public async Task<List<Topping>> GetToppingsAsync()
        {
            List<Topping> toppings;
            if(!_cache.TryGetValue("toppings", out toppings))
            {
                toppings = await _db.Toppings.OrderBy(t => t.Name).ToListAsync();
                _cache.Set("toppings",
                    toppings, DateTime.Now.AddMinutes(3));
                 // .CreateEntry("toppings").SetValue(toppings);               
            }
            return toppings;
        }
    }
}
