using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatiTechShop.Classes
{
    public class Parameter
    {
        public string? Name { get; set; }
        public string? Value { get; set; }
        public Parameter()
        {

        }
        public Parameter(string? name, string? value)
        {
            Name = name;
            Value = value;
        }
    }
}
