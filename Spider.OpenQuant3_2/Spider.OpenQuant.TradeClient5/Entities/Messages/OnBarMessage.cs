using EasyNetQ;

namespace Spider.OpenQuant.TradeClient5.Entities.Messages
{
    [Queue("QuoteServer.OnBar", ExchangeName = "QuoteServer.OnBar")]
    public class OnBarMessage
    {
        public SpiderInstrument Instrument { get; set; }

        public SpiderBar Bar { get; set; }
    }
}
