using System.ComponentModel.DataAnnotations.Schema;

namespace PPL3_Banhangonline.Models
{
    public class Seller
    {
        public int SellerID { get; set; }
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public int? Age { get; set; }
        [ForeignKey("UserID")]
        public Account? Account { get; set; }
        public Shop? Shop { get; set; }
    }
}
