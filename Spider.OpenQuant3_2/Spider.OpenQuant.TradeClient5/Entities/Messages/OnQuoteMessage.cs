using EasyNetQ;

namespace Spider.OpenQuant.TradeClient5.Entities.Messages
{
    [Queue("QuoteServer.OnQuote", ExchangeName = "QuoteServer.OnQuote")]
    public class OnQuoteMessage
    {
        public SpiderInstrument Instrument { get; set; }

        public SpiderQuote Quote { get; set; }
    }
}