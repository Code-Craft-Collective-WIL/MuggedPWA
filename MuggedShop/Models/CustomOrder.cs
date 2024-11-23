using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MuggedShop.Models
{
    public partial class CustomOrder
    {
        public int CustomOrderId { get; set; }
        public int? ProductId { get; set; }
        public string UserEmail { get; set; }

        [Display(Name = "Custom Image")]
        public string CustomImageUrl { get; set; }

        [Display(Name = "Custom Text")]
        public string CustomText { get; set; }
        public decimal? Price { get; set; }
        public int? Quantity { get; set; }
        public int OrderId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string Status { get; set; }

        public virtual Product Product { get; set; }
        public virtual User UserEmailNavigation { get; set; }
    }
}
