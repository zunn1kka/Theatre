namespace Theatre.Core.Models
{
    public class UserData
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public int TypeId { get; set; }
        public decimal Balance { get; set; }
    }
}
