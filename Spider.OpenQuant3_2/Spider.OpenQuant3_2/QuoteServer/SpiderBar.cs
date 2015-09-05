using System;
using OpenQuant.API;

namespace Spider.OpenQuant3_2.QuoteServer
{
    public class SpiderBar
    {
        public bool IsComplete { get; set; }

        public DateTime BeginTime { get; set; }

        public double Close { get; set; }

        public virtual DateTime EndTime { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Open { get; set; }

        public long Size { get; set; }

        public BarType Type { get; set; }

        public long Volume { get; set; }

    }
}