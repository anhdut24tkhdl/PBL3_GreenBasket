using System.ComponentModel.DataAnnotations.Schema;

namespace PPL3_Banhangonline.Models
{
    public class Product
    {
        public int ProductID { get; set; }
        public int ShopID { get; set; }
        public int CategoryID { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public int? Stock { get; set; }
        public string? Image { get; set; }
        [ForeignKey("ShopID")]
        public Shop? Shop { get; set; }
        public Category? Category { get; set; }
        public Price? Price { get; set; }

        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}
