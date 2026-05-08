using PPL3_Banhangonline.Models;

namespace PPL3_Banhangonline.Models.Viewmodels
{
    public class CustomerOrdersViewModel
    {
        public List<Order> RegularOrders { get; set; } = new();
        public List<RescueRegistration> RescueRegistrations { get; set; } = new();
        public string? CurrentStatus { get; set; }
        public DateTime? CurrentFromDate { get; set; }
        public DateTime? CurrentToDate { get; set; }
        public decimal? CurrentMinAmount { get; set; }
        public decimal? CurrentMaxAmount { get; set; }
        public string? CurrentKeyword { get; set; }
    }
}
