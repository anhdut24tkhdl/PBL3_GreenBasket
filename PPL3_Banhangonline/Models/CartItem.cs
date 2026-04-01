using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PPL3_Banhangonline.Models
{
    public class CartItem
    {
        [Key]
        public int CartItemID { get; set; }

        public int CartID { get; set; }

        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        [ForeignKey("CartID")]
        public Cart Cart { get; set; }

        [ForeignKey("ProductID")]
        public Product Product { get; set; }
        //public object Price { get; internal set; }
    }
}