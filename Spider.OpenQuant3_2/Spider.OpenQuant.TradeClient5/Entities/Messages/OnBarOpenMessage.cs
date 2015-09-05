using EasyNetQ;

namespace Spider.OpenQuant.TradeClient5.Entities.Messages
{
    [Queue("QuoteServer.OnBarOpen", ExchangeName = "QuoteServer.OnBarOpen")]
    public class OnBarOpenMessage
    {
        public SpiderInstrument Instrument { get; set; }

        public SpiderBar Bar { get; set; }
    }
}