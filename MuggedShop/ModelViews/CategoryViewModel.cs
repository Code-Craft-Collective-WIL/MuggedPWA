using MuggedShop.Models;

namespace MuggedShop.ModelViews
{
    // View model representing a product category with associated subcategories (Troeslen & Japikse, 2021)
    public class CategoryViewModel
    {
        public string CategoryName { get; set; } // Name of the category (Troeslen & Japikse, 2021)
        public int ProductCount { get; set; } // Number of products in the category(Troeslen & Japikse, 2021)
        public IEnumerable<SubcategoryViewModel> Subcategories { get; set; } // List of subcategories within the category (Troeslen & Japikse, 2021)
    }

    // View model representing a subcategory within a category, with associated products (Troeslen & Japikse, 2021)
    public class SubcategoryViewModel
    {
        public string SubcategoryName { get; set; } // Name of the subcategory (Troeslen & Japikse, 2021)
        public int SubcategoryProductCount { get; set; } // Number of products in the subcategory (Troeslen & Japikse, 2021)
        public IEnumerable<Product> Products { get; set; } // List of products under this subcategory (Troeslen & Japikse, 2021)
    }
}
