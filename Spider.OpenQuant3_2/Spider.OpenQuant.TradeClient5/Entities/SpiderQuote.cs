using System;

namespace Spider.OpenQuant.TradeClient5.Entities
{
    public class SpiderQuote
    {
        public double Ask { get; set; }

        public int AskSize { get; set; }

        public double Bid { get; set; }

        public int BidSize { get; set; }

        public DateTime DateTime { get; set; }

    }
}