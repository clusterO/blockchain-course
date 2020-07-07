using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class User
    {
        public string Ip { get; set; }
        public int Id { get; set; }
        public virtual List<Item> Items { get; set; }
    }

    public static class OwnedItem
    {
        public static List<User> users = new List<User>();
        public static List<User> AddUser(string ip, int id)
        {
            if(users.FirstOrDefault(x => x.Ip == ip && x.Id == id) == null)
            {
                var user = new User { Ip = ip, Id = id };
                users.Add(user);
            }

            return users;
        }
    }
}
