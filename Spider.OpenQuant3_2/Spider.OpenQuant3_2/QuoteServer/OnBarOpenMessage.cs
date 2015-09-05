using EasyNetQ;
using OpenQuant.API;

namespace Spider.OpenQuant3_2.QuoteServer
{
    [Queue("QuoteServer.OnBarOpen", ExchangeName = "QuoteServer.OnBarOpen")]
    public class OnBarOpenMessage
    {
        public SpiderInstrument Instrument { get; set; }

        public SpiderBar Bar { get; set; }
    }
}