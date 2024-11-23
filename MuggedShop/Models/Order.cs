using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MuggedShop.Models
{
    public partial class Order
    {
        public Order()
        {
            OrderItems = new HashSet<OrderItem>();
        }

        public int OrderId { get; set; }
        public string Email { get; set; }

        [Display(Name = "Fullname")]
        public string FullName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }

        [Display(Name = "Card Name")]
        public string CardName { get; set; }

        [Display(Name = "Card Number")]
        public string CardNumber { get; set; }

        [Display(Name = "Expire Month")]
        public string ExpMonth { get; set; }

        [Display(Name = "Expire Year")]
        public string ExpYear { get; set; }

        [Display(Name = "CVV")]
        public string Cvv { get; set; }

        [Display(Name = "Order Date")]
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedDate { get; set; }

        public string Status { get; set; }

        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
