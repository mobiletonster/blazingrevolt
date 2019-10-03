using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazingPizza.Data
{
    public class SpecialsService
    {
        private readonly PizzaStoreContext _db;
        public SpecialsService(PizzaStoreContext db)
        {
            _db = db;
        }

        public async Task<List<PizzaSpecial>> GetSpecialsAsync()
        {
            return await _db.Specials.OrderByDescending(s => s.BasePrice).ToListAsync();
        }

        public async Task<PizzaSpecial> GetPizzaSpecialAsync(int id)
        {
            return await _db.Specials.FirstOrDefaultAsync(m => m.Id == id);
        }
    }
}
