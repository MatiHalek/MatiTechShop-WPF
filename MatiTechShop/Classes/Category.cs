using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatiTechShop.Classes
{
    public class Category
    {
        public int Id { get; set; }
        public string ?Name { get; set; }
        public string ?Image { get; set; }
        public int Count
        {
            get
            {
                return DataAccess.CountProducts(Id);
            }

        }

        public Category(int id, string name, string image)
        {
            Id = id;
            Name = name;
            Image = image;
        }
        public Category() { }   
    }
}
