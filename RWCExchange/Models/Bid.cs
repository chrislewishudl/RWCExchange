using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace RWCExchange.Models
{
    public class Bid : IComparable<Bid>, IEquatable<Bid>
    {
        public int BidID { get; set; }
        public DateTime TimeStamp { get; set; }

        public int CountryID { get; set; }
        public virtual Country Country { get; set; }

        public int UserID { get; set; }
        public virtual User User { get; set; }

        public double Price { get; set; }
        public int CompareTo(Bid other)
        {
            if (Price > other.Price) return -1;
            if (Price < other.Price) return 1;
            if (TimeStamp < other.TimeStamp) return -1;
            return TimeStamp > other.TimeStamp ? 1 : 0;
        }

        public bool Equals(Bid other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return TimeStamp.Equals(other.TimeStamp) && CountryID == other.CountryID && Price.Equals(other.Price);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Bid) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TimeStamp.GetHashCode();
                hashCode = (hashCode*397) ^ CountryID;
                hashCode = (hashCode*397) ^ Price.GetHashCode();
                return hashCode;
            }
        }
    }
}