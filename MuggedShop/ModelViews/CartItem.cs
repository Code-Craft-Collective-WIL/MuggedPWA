namespace MuggedShop.ModelViews
{
    // Represents an item in the shopping cart (Troeslen & Japikse, 2021)
    public class CartItem
    {
        // Unique identifier for the product (Troeslen & Japikse, 2021)
        public int ProductId { get; set; }

        // Name of the product (Troeslen & Japikse, 2021)
        public string ProductName { get; set; }

        // URL of the product image (Troeslen & Japikse, 2021)
        public string ImageUrl { get; set; }

        // Price of the product (Troeslen & Japikse, 2021)
        public decimal Price { get; set; }

        // Quantity of the product added to the cart (Troeslen & Japikse, 2021)
        public int Quantity { get; set; }

        // Stock count of the product added to the cart (Troeslen & Japikse, 2021)
        public int StockCount { get; set; }
    }
}
