using System.ComponentModel.DataAnnotations.Schema;

namespace PPL3_Banhangonline.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        [ForeignKey("Account")]
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }

        public Account? Account { get; set; }
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
