using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MatiTechShop.Classes
{
    public class Order
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public DateTime Date { get; set; }
        public string? Delivery { get; set; }
        public string? Payment { get; set; }
        public string? Status { get; set; }
        public Order() { }

        public Order(string? username, DateTime date, string? delivery, string? payment, string? status)
        {
            Username = username;
            Date = date;
            Delivery = delivery;
            Payment = payment;
            Status = status;
        }
    }
}
