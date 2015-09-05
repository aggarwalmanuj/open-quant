using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;

namespace Spider.OpenQuant.TradeClient5.Entities
{
    public class SpiderPosition
    {
        public Instrument Instrument { get; set; }

        public PositionSide PositionSide { get; set; }

        public double Quantity { get; set; }
    }
}
