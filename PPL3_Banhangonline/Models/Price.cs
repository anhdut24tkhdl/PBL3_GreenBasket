namespace PPL3_Banhangonline.Models
{
    public class Price
    {
        public int PriceId { get; set; }
        public int ProductID { get; set; }
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }

        public Product Product { get; set; }
    }
}
