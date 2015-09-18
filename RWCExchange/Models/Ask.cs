using System;

namespace RWCExchange.Models
{
    public class Ask : IComparable<Ask>, IEquatable<Ask>
    {
        public int AskID { get; set; }
        public DateTime TimeStamp { get; set; }
        public int CountryID { get; set; }
        public virtual Country Country { get; set; }

        public double Price { get; set; }

        public int CompareTo(Ask other)
        {
            if (Price < other.Price) return -1;
            if (Price > other.Price) return 1;
            if (TimeStamp < other.TimeStamp) return -1;
            return TimeStamp > other.TimeStamp ? 1 : 0;
        }

        public bool Equals(Ask other)
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
            return Equals((Ask) obj);
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