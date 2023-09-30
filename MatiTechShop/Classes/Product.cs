using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatiTechShop.Classes
{
    public class Product
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? Promotion { get; set;}
        public string? BestFeature { get; set; }
        public int Quantity { get; set; }
        public int? Guarantee { get; set; }
        public Product()
        {

        }

        public Product(string? name, string? description, decimal price, decimal? promotion, string? bestFeature, int quantity, int? guarantee)
        {
            Name = name;
            Description = description;
            Price = price;
            Promotion = promotion;
            BestFeature = bestFeature;
            Quantity = quantity;
            Guarantee = guarantee;
        }
    }
}
