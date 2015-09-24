using System;

namespace RWCExchange.Models
{
    public class Trade
    {
        public int TradeID { get; set; }
        public double Price { get; set; }

        public int CountryID { get; set; }
        public virtual Country Country { get; set; }
        
        public int SellerID { get; set; }
        public virtual User Seller { get; set; }
        
        public int BuyerID { get; set; }
        public virtual User Buyer { get; set; }

        public DateTime TimeStamp { get; set; }
    }
}