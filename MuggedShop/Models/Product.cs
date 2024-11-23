using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace MuggedShop.Models
{
    public partial class Product
    {
        public Product()
        {
            CustomOrders = new HashSet<CustomOrder>();
            OrderItems = new HashSet<OrderItem>();
        }

        public int ProductId { get; set; }

        [Display(Name = "Category Name")]
        public int? CategoryId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public string Description { get; set; }

        [Display(Name = "Stock Count")]
        public int? StockCount { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; }

        public virtual Category Category { get; set; }
        public virtual ICollection<CustomOrder> CustomOrders { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; }
    }
}
