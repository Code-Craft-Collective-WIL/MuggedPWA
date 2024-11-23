using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MuggedShop.Models
{
    public partial class User
    {
        public User()
        {
            CustomOrders = new HashSet<CustomOrder>();
        }

        public string Email { get; set; }

        [Display(Name = "Fullname")]
        public string FullName { get; set; }

        [Display(Name = "Phone number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Password")]
        public string PasswordHash { get; set; }

        public virtual ICollection<CustomOrder> CustomOrders { get; set; }
    }
}
