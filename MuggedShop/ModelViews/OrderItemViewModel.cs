using System.ComponentModel.DataAnnotations;

namespace MuggedShop.Models
{
    // Represents the data model for individual order items with validation attributes (Troeslen & Japikse, 2021)
    public class OrderItemViewModel
    {
        // Name of the product in the order (Troeslen & Japikse, 2021)
        [Required]
        [Display(Name = "Product Name")]
        public string ProductName { get; set; }

        // Quantity of the product ordered (Troeslen & Japikse, 2021)
        [Required]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }


        // Price of the product ordered (Troeslen & Japikse, 2021)
        [Required]
        [Display(Name = "Price")]
        public decimal Price { get; set; }
    }
}