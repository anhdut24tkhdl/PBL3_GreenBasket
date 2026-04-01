namespace PPL3_Banhangonline.Models
{
    public class Account
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string Role { get; set; } = null!;
        
    }
}
