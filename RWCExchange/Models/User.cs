using System.Collections.Generic;

namespace RWCExchange.Models
{
    public class User
    {
        public int UserID { get; set; }
        public string UserName { get; set; }

        public virtual ICollection<Country> Countries { get; set; }

        public virtual ICollection<Trade> BuyTrades { get; set; } 
        public virtual ICollection<Trade> SellTrades { get; set; } 
    }
}