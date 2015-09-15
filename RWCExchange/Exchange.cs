using System;
using System.Collections.Generic;

namespace RWCExchange
{
    public class Trade
    {
        public Bid Bid { get; set; }
        public Ask Ask { get; set; }
        public double Price { get; set; }
    }

    public class Bid : IComparable<Bid>
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
    }

    public class Ask : IComparable<Ask>
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
    }

    public sealed class Exchange
    {
        private static readonly Lazy<Exchange> _instance = new Lazy<Exchange>(()=>new Exchange());

        private Dictionary<string, List<Bid>> _bids;
        private Dictionary<string, List<Ask>> _asks;

        private Dictionary<string, string> _owners; 

        private readonly List<string> _countries; 

        public static Exchange Instance
        {
            get { return _instance.Value; }
        }

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

        public bool IsCurrentOwner(string country, string user)
        {
            return _owners[country] == user;
        }

        public Trade AddBid(string country,Bid bid)
        {
            var lowestAsk = _asks[country][0];
            if (bid.Price >= lowestAsk.Price && lowestAsk.User != bid.User)
            {
                _asks[country].Remove(lowestAsk);
                _owners[country] = bid.User;
                return new Trade {Bid = bid, Ask = lowestAsk, Price = bid.Price};
            }
            _bids[country].Add(bid);
            _bids[country].Sort();
            return null;
        }

        public Trade AddAsk(string country, Ask ask)
        {
            var highestBid = _bids[country][0];
            if (highestBid.Price >= ask.Price && ask.User != highestBid.User)
            {
                _bids[country].Remove(highestBid);
                _owners[country] = ask.User;
                return new Trade { Bid = highestBid, Ask = ask, Price = ask.Price };
            }
            _asks[country].Add(ask);
            _asks[country].Sort();
            return null;
        }

        public Bid GetBestBid(string country)
        {
            return _bids[country][0];
        }

        public Ask GetBestAsk(string country)
        {
            return _asks[country][0];
        }


    }
}