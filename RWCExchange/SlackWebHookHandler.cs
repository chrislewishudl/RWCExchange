using System.Collections.Specialized;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNet.WebHooks;

namespace RWCExchange
{
    public class SlackWebHookHandler : WebHookHandler
    {
        private Regex _regex = new Regex(@"^\:(?<bs>buy|sell)\s+(?<country>[A-Za-z]{3})\s+\@\s+(?<price>[0-9]+(?:\.[0-9]{2})?)");
        private Regex _regexConfig = new Regex(@"^:show\s+(?<show>(bids|offers|countries|owners))");
        private Task ReturnMessage(string message, WebHookHandlerContext context)
        {
            context.Response = context.Request.CreateResponse(new SlackResponse(message));
            return Task.FromResult(true);
        }


        public override Task ExecuteAsync(string receiver, WebHookHandlerContext context)
        {
            NameValueCollection nvc;
            if (context.TryGetData(out nvc))
            {
                var user = nvc["user_name"];
                if (string.IsNullOrEmpty(user))
                {
                    return ReturnMessage("Invalid User! Sorry....",context);
                }
                var text = nvc["text"];
                var match1 = _regex.Match(text);
                var match2 = _regexConfig.Match(text);
                if (!match1.Success && !match2.Success)
                {
                    return ReturnMessage("Invalid Syntax! try \"(:buy|:sell) CountryCode @ Price\" to make a buy or sell request.\nYou can also type \":show bids|offers|countries|owners\" to show exchange info.",context);
                }
                if (match1.Success)
                {
                    var isBuy = match1.Groups["bs"].Value == "buy";
                    var country = match1.Groups["country"].Value;
                    var price = match1.Groups["price"].Value;
                    if (!Exchange.Instance.IsValidCountry(country))
                    {
                        return ReturnMessage("Invalid country code. Try again!", context);
                    }
                    var isCurrentOwner = Exchange.Instance.IsCurrentOwner(country, user);
                    if (isBuy && isCurrentOwner) return ReturnMessage($"{user} you already own {country}! You can't buy it again you fool!",context);
                    if (!isBuy && !isCurrentOwner)return ReturnMessage($"{user} you dont own {country}. You can't sell something you don't own you fool!",context);

                    
                }
                else
                {
                    
                }
            }
        }
    }
}