using MuggedShop.ModelViews;

namespace MuggedShop.Models
{
    // View model for checkout, encapsulating an order and the associated cart items (Troeslen & Japikse, 2021)
    public class CheckoutViewModel
    {
        // Represents the order being checked out (Troeslen & Japikse, 2021)
        public Order Order { get; set; }

        // List of items in the shopping cart associated with the order (Troeslen & Japikse, 2021)
        public List<CartItem> CartItems { get; set; }
    }
}
