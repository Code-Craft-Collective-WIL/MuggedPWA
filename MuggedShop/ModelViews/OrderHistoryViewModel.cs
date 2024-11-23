using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MuggedShop.Models
{
    // Represents the data model for displaying a user's order history, including order details and items (Troeslen & Japikse, 2021)
    public class OrderHistoryViewModel
    {
        // Unique identifier for the order (Troeslen & Japikse, 2021)
        [Key]
        [Required]
        [Display(Name = "Order ID")]
        public int OrderId { get; set; }

        // Date and time when the order was created (Troeslen & Japikse, 2021)
        [Display(Name = "Created At")]
        public DateTime? CreatedAt { get; set; }

        // Full name of the customer who placed the order (Troeslen & Japikse, 2021)
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        // Email address of the customer (Troeslen & Japikse, 2021)
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        // Status of the order (Pending, Processed, etc.)
        [Display(Name = "Status")]
        public string Status { get; set; }

        // The date when the order was processed (if applicable)
        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }


        // Total amount for the order (Troeslen & Japikse, 2021)
        [Display(Name = "Total Amount")]
        public decimal TotalAmount { get; set; }

        // List of items included in the order (Troeslen & Japikse, 2021)
        public List<OrderItemViewModel> OrderItems { get; set; }
    }
}