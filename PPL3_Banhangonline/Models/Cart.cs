using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPL3_Banhangonline.Models
{
    public class Cart
    {
        [Key]

        public int CartID { get; set; }
        public int CustomerID { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [ForeignKey("CustomerID")]
        public Customer? Customer { get; set; }
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
