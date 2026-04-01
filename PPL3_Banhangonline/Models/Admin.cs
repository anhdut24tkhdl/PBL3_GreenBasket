namespace PPL3_Banhangonline.Models
{
    public class Admin
    {
        public int AdminID { get; set; }
        public int UserID { get; set; }
        public string? Name { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }

        public Account? Account { get; set; }
    }
}
