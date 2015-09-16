using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace RWCExchange
{
    public class SlackWebHookHandler : WebHookHandler
    {
        private readonly Regex _regex = new Regex(@"^\:(?<bs>buy|sell)\s+(?<country>[A-Za-z]{3})\s*\@\s*(?<price>[0-9]+(?:\.[0-9]{2})?)");
        private readonly Regex _regexConfig = new Regex(@"^\:show\s+(?:(?<showc>bids|offers|market)\s+|(?<show>owners|countries))(?(1)(?<country>[A-Za-z]{3})|\.*)");
        private readonly Regex _regexRemove = new Regex(@"^\:drop\s+(?<team>[A-Za-z]{3})");
        private readonly Regex _regexOwner = new Regex(@"^\:owner\s+(?<team>[A-Za-z]{3})\s+(?<user>[A-Za-z]+)");
        private readonly Regex _regexPull = new Regex(@"\:pull\s+(?<bs>buy|sell)\s+(?<country>[A-Za-z]{3})");

        private Task ReturnMessage(string message, WebHookHandlerContext context)
        {
            context.Response = context.Request.CreateResponse(new SlackResponse(message));
            return Task.FromResult(true);
        }


        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            try
            {
                NameValueCollection nvc;
                if (context.TryGetData(out nvc))
                {
                    var user = nvc["user_name"];
                    if (string.IsNullOrEmpty(user))
                    {
                        return ReturnMessage("Invalid User! Sorry....", context);
                    }
                    var text = nvc["text"];
                    var tradeMatch = _regex.Match(text);
                    var showMatch = _regexConfig.Match(text);
                    var removeMatch = _regexRemove.Match(text);
                    var ownerMatch = _regexOwner.Match(text);
                    var pullMatch = _regexPull.Match(text);
                    if (!tradeMatch.Success && !showMatch.Success && !removeMatch.Success && !ownerMatch.Success && !pullMatch.Success && !text.Trim().Contains(":help"))
                    {
                        return ReturnMessage("Invalid Syntax! try \":help\"", context);
                    }
                    if (tradeMatch.Success)
                    {
                        if (!tradeMatch.Groups["bs"].Success || !tradeMatch.Groups["country"].Success ||
                            !tradeMatch.Groups["price"].Success)
                            return ReturnMessage("Failed to parse the trade request sorry!", context);

                        var price = tradeMatch.Groups["price"].Value;
                        double priced;
                        return !double.TryParse(price, out priced) || priced<=0 ? ReturnMessage("Invalid price!", context) : RunTrade(tradeMatch.Groups["bs"].Value == "buy", tradeMatch.Groups["country"].Value.ToUpper(), user, priced, context);
                    }
                    if (showMatch.Success)
                    {
                        return showMatch.Groups["showc"].Success ? RunShow(showMatch.Groups["showc"].Value, context, showMatch.Groups["country"].Value.ToUpper()) : RunShow(showMatch.Groups["show"].Value, context);
                    }
                    if (pullMatch.Success)
                    {
                        return RunPull(pullMatch.Groups["bs"].Value == "buy", pullMatch.Groups["country"].Value.ToUpper(), user,
                            context);
                    }
                    if (text.Trim().Contains(":help"))
                    {
                        return RunHelp(context);
                    }
                    if (user != "chrislewis") return ReturnMessage("Permission denied! Don't be so cheeky..", context);
                    if (removeMatch.Success)
                    {
                        return RunDrop(removeMatch.Groups["team"].Value.ToUpper(), context);
                    }
                    if (ownerMatch.Success)
                    {
                        return RunOwner(ownerMatch.Groups["team"].Value.ToUpper(), ownerMatch.Groups["user"].Value, context);
                    }
                }
                return Task.FromResult(true);
            }
            catch (Exception e)
            {
                return ReturnMessage(e.Message,context);
            }
        }

        private Task RunTrade(bool isBuy, string country, string user, double price, WebHookHandlerContext context)
        {
            if (!Exchange.Instance.IsValidCountry(country))
            {
                return ReturnMessage("Invalid country code. Try again!", context);
            }
            if (Exchange.Instance.CountryDropped(country))
            {
                return ReturnMessage($"This country is already out of the cup. Come on, keep up! Go {(isBuy?"buy":"sell")} another one.",context);
            }
            var isCurrentOwner = Exchange.Instance.IsCurrentOwner(country, user);
            if (isBuy && isCurrentOwner) return ReturnMessage($"@{user} you already own {country}! You can't buy it again you fool!", context);
            if (!isBuy && !isCurrentOwner) return ReturnMessage($"@{user} you dont own {country}. You can't sell something you don't own you fool!", context);
            if (isBuy)
            {
                if(!Exchange.Instance.CurrentlyOwned(country))return ReturnMessage($"{country} doesn't yet have an owner, wait until it does before you start to place bids.",context);
                var trade = Exchange.Instance.AddBid(country,new Bid { Price = price, TimeStamp = DateTime.UtcNow, User = user });
                if (trade?.Bid != null && trade.Ask!=null)
                {
                    return ReturnMessage($"WOOOO! You traded! @{user} you now own {country}, and you owe @{trade.Ask.User} *£{trade.Price}*. Pay up or I'll send the bailiffs!", context);
                }
                if (trade != null && trade.Update)
                {
                    return ReturnMessage($"@{user} you're buy request has been updated for {country}. Let's hope we can get a matching sale price.",context);
                }
                return ReturnMessage("You're bid has been accepted, but it's not triggered a trade yet. Fingers crossed for you!", context);
            }
            else
            {
                var trade = Exchange.Instance.AddAsk(country, new Ask{ Price = price, TimeStamp = DateTime.UtcNow, User = user });
                if (trade?.Bid != null && trade.Ask != null)
                {
                    return ReturnMessage($"WOOOO! You traded! @{trade.Bid.User} you now own {country}, and you owe @{user} *£{trade.Price}*. Pay up or I'll send the bailiffs!", context);
                }
                if (trade != null && trade.Update)
                {
                    return ReturnMessage($"@{user} you're sell request has been update for {country}. Let's wait for a buyer!",context);
                }
                return ReturnMessage("You're sell offer has been accepted but it's not triggered a trade yet. Fingers crossed for you!", context);
            }
        }

        private Task RunShow(string showWhat, WebHookHandlerContext context, string country = null)
        {
            if (country != null)
            {
                if (!Exchange.Instance.IsValidCountry(country))
                {
                    return ReturnMessage("Invalid country code. Try again!", context);
                }
                if (Exchange.Instance.CountryDropped(country))
                {
                    return ReturnMessage("This country is already out of the cup. Come on, keep up!", context);
                }
                switch (showWhat)
                {
                    case "bids":
                        return ReturnMessage($"*Bids for {country}:*\n" + string.Join("\n", Exchange.Instance.GetBids(country).Select(i => $"Price: *{i.Price}*, User: @{i.User}")), context);
                    case "offers":
                        return ReturnMessage($"*Offers for {country}:*\n" + string.Join("\n", Exchange.Instance.GetAsks(country).Select(i => $"Price: *{i.Price}*, User: @{i.User}")), context);
                    case "market":
                        var bestBid = Exchange.Instance.GetBestBid(country);
                        var bestAsl = Exchange.Instance.GetBestAsk(country);
                        var bidString = bestBid == null ? "_No Bids_" : $"Bid Price: *{bestBid.Price}* @{bestBid.User}";
                        var askString = bestAsl == null ? "_No asks_" : $"Ask Price: *{bestAsl.Price}* @{bestAsl.User}";
                        return ReturnMessage($"*Best Market for {country}:*\n" + bidString + "     " + askString, context);
                }
            }
            var owners = Exchange.Instance.GetOwners();
            switch (showWhat)
            {
                case "owners":
                    return ReturnMessage("*Current Owners:*\n"+string.Join("\n",owners.Select(i=>$"{i.Key}" +"     "+ (i.Value==string.Empty?"_none_":"@"+i.Value))),context);
                case "countries":
                    return ReturnMessage("*Countries Left in the Tournament:*\n" + string.Join("\n", owners.Select(i => $"{i.Key}")),context);
            }
            return Task.FromResult(true);

        }

        private Task RunDrop(string country, WebHookHandlerContext context)
        {
            if (!Exchange.Instance.IsValidCountry(country))
            {
                return ReturnMessage("Invalid country code. Try again!", context);
            }
            if (Exchange.Instance.CountryDropped(country))
            {
                return ReturnMessage("This country is already out of the cup. Come on, keep up!", context);
            }
            var user = Exchange.Instance.DropTeam(country);
            return ReturnMessage($"{country} has been dropped from the tournament. Sorry @{user}",context);
        }

        private Task RunOwner(string country, string owner, WebHookHandlerContext context)
        {
            if (!Exchange.Instance.IsValidCountry(country))
            {
                return ReturnMessage("Invalid country code. Try again!", context);
            }
            if (Exchange.Instance.CountryDropped(country))
            {
                return ReturnMessage("This country is already out of the cup. Come on, keep up!", context);
            }
            if (Exchange.Instance.SetOwner(country, owner))
            {
                ReturnMessage($"Congrats @{owner} You now own {country}.", context);
            }
            return Task.FromResult(true);
        }

        private Task RunPull(bool isBid, string country, string user, WebHookHandlerContext context)
        {
            if (!Exchange.Instance.IsValidCountry(country))
            {
                return ReturnMessage("Invalid country code. Try again!", context);
            }
            if (Exchange.Instance.CountryDropped(country))
            {
                return ReturnMessage("This country is already out of the cup. Come on, keep up!", context);
            }
            if (!isBid && !Exchange.Instance.IsCurrentOwner(country, user))
            {
                return ReturnMessage($"You aren't the current owner of {country}, so you have no offers in the market.",context);
            }
            if (isBid)
            {
                if (Exchange.Instance.PullBid(country, user))
                    return ReturnMessage($"Thanks @{user} You're bid for {country} has been dropped from the market.", context);
                ReturnMessage($"{user} - Pull failed. Do you even have a bid in the market?", context);
            }
            if(Exchange.Instance.PullAsk(country,user))return ReturnMessage($"Thanks @{user} You're offer to sell {country} has been dropped from the market.",context);
            return ReturnMessage($"@{user} - Pull failed. Do you even have a sell offer in the market?", context);
        }

        private Task RunHelp(WebHookHandlerContext context)
        {
            var sb = new StringBuilder("*Help:*\n");
            sb.AppendLine(
                "To make a bid or request a sale - \":<buy|sell> <countryCode> @ <price>\" - Note: you can only buy a team you don't own and you can only sell one that you do own.\n" +
                "                                                                                  The exchange will make a trade if you're price crossed the market best bid/ask. \n" +
                "                                                                                  The price of the trade will be the price you enter, so check first!\n" +
                "                                                                                  If 2 or more people have bids in at the same price then its first come first serve.\n" +
                "                                                                                  If you already have a bid or offer in the market then this command will update the price and time on that bid or offer.");
            sb.AppendLine("To show current best prices for country - \":show market <countryCode>\"");
            sb.AppendLine("To show all bids/offers on country - \":show <bids|offers> <countryCode>\"");
            sb.AppendLine("To show current ownership - \":show owners\"");
            sb.AppendLine("To show all remaining teams - \":show countries\"");
            sb.AppendLine("To remove you're current bid or ask - \":pull <buy|sell> <countryCode>\"");
            return ReturnMessage(sb.ToString(),context);
        }



    }
}