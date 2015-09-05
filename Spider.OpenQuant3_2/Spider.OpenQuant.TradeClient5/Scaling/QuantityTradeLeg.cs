using System;

using NLog;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Scaling
{
    public class QuantityTradeLeg : BaseTradeLeg
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public double QuantityToFill { get; set; }

        public QuantityTradeLeg(int idx, Instrument instrument, OrderSide orderSide, double qty)
            : base(idx, instrument, orderSide)
        {
            this.QuantityToFill = qty;
            this.LegName = string.Format("Q{0}.{1}.{2}.{3}",
                this.Index,
                this.Instrument.Symbol.ToUpper(),
                this.OrderSide,
                this.QuantityToFill);

            Log.WriteInfoLog(LegName,
               this.Instrument,
               string.Format("Quantity leg generated to {0} {1:N2} units", this.OrderSide.ToString().ToUpper(), this.QuantityToFill));
        }

        public override bool IsLegComplete
        {
            get
            {
                return FilledQuantity >= QuantityToFill;
            }
        }

        protected override string GetLegNamePrefix()
        {
            return "Q";
        }

        public override double GetQuantityForNewOrder(double price)
        {
            return QuantityToFill - FilledQuantity;
        }


        public override double GetQuantityForExitingOrder(double price)
        {
            return OrderTracker.QuantityToFill;
        }
    }
}