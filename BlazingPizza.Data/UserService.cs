using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlazingPizza.Data
{

    public class UserService
    {
        private readonly PizzaStoreContext _db;
        public 
            UserService(PizzaStoreContext db)
        {
            _db = db;
        }

        public Address GetUserRecentDeliveryAddress(string userId)
        {
            var address = _db.Address.Where(m => m.UserId == userId).OrderByDescending(m=>m.Id).FirstOrDefault();
            return address;
            //return null;
        }
    }
}
