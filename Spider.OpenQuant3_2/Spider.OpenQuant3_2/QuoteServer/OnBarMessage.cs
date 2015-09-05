using System.Collections.Generic;
using System.Linq;
using System.Text;
using EasyNetQ;
using OpenQuant.API;

namespace Spider.OpenQuant3_2.QuoteServer
{
    [Queue("QuoteServer.OnBar", ExchangeName = "QuoteServer.OnBar")]
    public class OnBarMessage
    {
        public SpiderInstrument Instrument { get; set; }

        public SpiderBar Bar { get; set; }
    }
}
