using EasyNetQ;
using OpenQuant.API;

namespace Spider.OpenQuant3_2.QuoteServer
{
    [Queue("QuoteServer.OnQuote", ExchangeName = "QuoteServer.OnQuote")]
    public class OnQuoteMessage
    {
        public SpiderInstrument Instrument { get; set; }

        public SpiderQuote Quote { get; set; }
    }
}