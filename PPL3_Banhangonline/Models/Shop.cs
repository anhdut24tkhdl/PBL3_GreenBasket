namespace PPL3_Banhangonline.Models
{
    public class Shop
    {
        public int ShopID { get; set; }
        public int SellerID { get; set; }
        public string? ShopName { get; set; }

        public Seller? Seller { get; set; }
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
