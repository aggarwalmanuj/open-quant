using System;

namespace Spider.OpenQuant3_2.QuoteServer
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