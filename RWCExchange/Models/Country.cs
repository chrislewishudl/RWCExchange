using System;
using System.Collections.Generic;

namespace RWCExchange.Models
{
    public class Country : IEquatable<Country>
    {
        public int CountryID { get; set; }
        public string Code { get; set; }

        public bool IsDropped { get; set; }

        public bool Equals(Country other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return CountryID == other.CountryID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Country) obj);
        }

        public override int GetHashCode()
        {
            return CountryID;
        }

        public int UserID { get; set; }
        public virtual User User{ get; set; }

        public virtual ICollection<Bid> Bids { get; set; }
        
        public virtual Ask Ask { get; set; } 
    }
}