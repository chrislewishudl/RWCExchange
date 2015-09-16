using System;
using System.Collections.Generic;
using System.Linq;

namespace RWCExchange
{
    public class Trade
    {
        public Bid Bid { get; set; }
        public Ask Ask { get; set; }
        public double Price { get; set; }
        public bool Update { get; set; }
    }

    public class Bid : IComparable<Bid>, IEquatable<Bid>
    {
        public DateTime TimeStamp { get; set; }
        public string User { get; set; }
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
            return TimeStamp.Equals(other.TimeStamp) && string.Equals(User, other.User) && Price.Equals(other.Price);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Bid)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = TimeStamp.GetHashCode();
                hashCode = (hashCode * 397) ^ (User != null ? User.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Price.GetHashCode();
                return hashCode;
            }
        }

    }

    public class Ask : IComparable<Ask>, IEquatable<Ask>
    {
        public DateTime TimeStamp { get; set; }
        public string User { get; set; }
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
            return TimeStamp.Equals(other.TimeStamp) && string.Equals(User, other.User) && Price.Equals(other.Price);
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
                hashCode = (hashCode*397) ^ (User != null ? User.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Price.GetHashCode();
                return hashCode;
            }
        }
    }

    public sealed class Exchange
    {
        private static readonly Lazy<Exchange> _instance = new Lazy<Exchange>(()=>new Exchange());

        private readonly Dictionary<string, List<Bid>> _bids;
        private readonly Dictionary<string, List<Ask>> _asks;

        private readonly Dictionary<string, string> _owners; 

        private readonly List<string> _countries; 

        public static Exchange Instance => _instance.Value;

        private Exchange()
        {
            _countries = new List<string> { "ARG", "AUS", "CAN", "ENG", "FJI", "FRA", "GEO", "IRE", "ITA", "JPN", "NAM", "NZL", "ROM", "SAM", "SCO", "RSA", "TGA", "URU", "USA", "WAL" };
            _bids = new Dictionary<string, List<Bid>>();
            _asks = new Dictionary<string, List<Ask>>();
            _owners = new Dictionary<string, string>();
            foreach (var c in _countries)
            {
                _bids.Add(c,new List<Bid>());
                _asks.Add(c,new List<Ask>());
                _owners.Add(c,string.Empty);
            }
        }

        public bool IsValidCountry(string country)
        {
            return _countries.Contains(country);
        }

        public bool CountryDropped(string country)
        {
            return !_owners.ContainsKey(country);
        }

        public bool IsCurrentOwner(string country, string user)
        {
            return _owners[country] == user;
        }

        public List<Bid> GetBids(string country)
        {
            return _bids[country].ToList();
        }

        public List<Ask> GetAsks(string country)
        {
            return _asks[country].ToList();
        }

        public List<KeyValuePair<string, string>> GetOwners()
        {
            return _owners.ToList();
        } 

        public Trade AddBid(string country,Bid bid)
        {
            var existingForUser = _bids[country].FirstOrDefault(i => i.User == bid.User);
            var trade = new Trade();
            if (existingForUser != null)
            {
                existingForUser.TimeStamp = bid.TimeStamp;
                existingForUser.Price = bid.Price;
                trade.Update = true;
                _bids[country].Sort();
                if (!_asks[country].Any() || !_bids[country][0].Equals(bid))
                {
                    return trade;
                }
            }
            if (!_asks[country].Any() && !trade.Update)
            {
                _bids[country].Add(bid);
                _bids[country].Sort();
                return trade;
            }
            var lowestAsk = _asks[country][0];
            if (bid.Price >= lowestAsk.Price)
            {
                _asks[country].Clear();
                _owners[country] = bid.User;
                trade.Bid = bid;
                trade.Ask = lowestAsk;
                trade.Price = bid.Price;
                trade.Update = false;
                return trade;
            }
            if (!trade.Update)
            {
                _bids[country].Add(bid);
                _bids[country].Sort();
            }
            return trade;
        }

        public Trade AddAsk(string country, Ask ask)
        {
            var existingForUser = _asks[country].FirstOrDefault(i => i.User == ask.User);
            var trade = new Trade();
            if (existingForUser != null)
            {
                existingForUser.TimeStamp = ask.TimeStamp;
                existingForUser.Price = ask.Price;
                trade.Update = true;
                if (!_bids[country].Any())
                {
                    return trade;
                }
            }
            if (!_bids[country].Any() && !trade.Update)
            {
                _asks[country].Add(ask);
                return trade;
            }
            var highestBid = _bids[country][0];
            if (highestBid.Price >= ask.Price)
            {
                _bids[country].Remove(highestBid);
                _owners[country] = highestBid.User;
                trade.Bid = highestBid;
                trade.Ask = ask;
                trade.Price = ask.Price;
                trade.Update = false;
                return trade;
            }
            if (!trade.Update)
            {
                _asks[country].Add(ask);
            }
            return trade;
        }

        public Bid GetBestBid(string country)
        {
            return !_bids[country].Any()?null:_bids[country][0];
        }

        public Ask GetBestAsk(string country)
        {
            return !_asks[country].Any()?null:_asks[country][0];
        }

        public string DropTeam(string country)
        {
            _bids[country].Clear();
            _asks[country].Clear();
            var current = _owners[country];
            _owners.Remove(country);
            return current;
        }

        public bool CurrentlyOwned(string country)
        {
            return _owners[country] != string.Empty;
        }

        public bool PullBid(string country, string user)
        {
            var bid = _bids[country].FirstOrDefault(i => i.User == user);
            if (bid == null) return false;
            _bids[country].Remove(bid);
            _bids[country].Sort();
            return true;
        }

        public bool PullAsk(string country, string user)
        {
            var ask = _asks[country].FirstOrDefault(i => i.User == user);
            if (ask == null) return false;
            _asks[country].Remove(ask);
            return true;
        }

        public bool SetOwner(string country, string user)
        {
            _owners[country] = user;
            if (user == "house")
            {
                _asks[country].Clear();
                _asks[country].Add(new Ask() {Price = 0.01,TimeStamp = DateTime.Now,User = "house"});
            }
            return true;
        }



    }
}