namespace MuggedShop.ModelViews
{
    public class CustomOrderHistoryViewModel
    {
        public int CustomOrderId { get; set; } // Unique identifier for the custom order (Troeslen & Japikse, 2021)
        public DateTime CreatedAt { get; set; } // The date the custom order was created (Troeslen & Japikse, 2021)
        public string Status { get; set; } // The current status of the order (Troeslen & Japikse, 2021)
        public decimal TotalAmount { get; set; } // The total amount for this custom order (Troeslen & Japikse, 2021)
        public List<CustomOrderItemViewModel> CustomOrderItems { get; set; } // List of items in the order (Troeslen & Japikse, 2021)
    }

}
