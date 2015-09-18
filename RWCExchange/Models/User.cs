namespace RWCExchange.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }

        public int CountryID { get; set; }
        public virtual Country Country { get; set; }
    }
}