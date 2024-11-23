using MuggedShop.Models;

namespace MuggedShop.ModelViews
{
    public class OrderDetailsViewModel
    {
        public int OrderId { get; set; } // Unique identifier for the order (Troeslen & Japikse, 2021)

        public DateTime? CreatedAt { get; set; } // Date and time when the order was created (nullable) (Troeslen & Japikse, 2021)
        public DateTime? ProcessedDate { get; set; } // Date and time when the order was processed (nullable) (Troeslen & Japikse, 2021)

        public string UserEmail { get; set; } // Email address of the user who placed the order (Troeslen & Japikse, 2021)

        public List<OrderItemViewModel> OrderItems { get; set; } // List of items included in the order (Troeslen & Japikse, 2021)
    }
}
