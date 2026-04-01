namespace PPL3_Banhangonline.Models
{
    public class Category
    {
        public int CategoryID { get; set; }
        public string? CategoryName { get; set; }
        public string? Image { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
