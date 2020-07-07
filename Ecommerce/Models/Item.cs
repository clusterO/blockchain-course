using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ecommerce.Models
{
    public class Item
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public virtual List<User> Users { get; set; }
    }

    public static class ListItem
    {
        public static List<Item> Items()
        {
            var listItem = new List<Item>
            {
                new Item {Id = 1, Title = "", Url = "", Image = "", Price = 0, Users = new List<User>()},
                new Item {Id = 2, Title = "", Url = "", Image = "", Price = 0, Users = new List<User>()}
            };

            return listItem;
        }
    }
}
