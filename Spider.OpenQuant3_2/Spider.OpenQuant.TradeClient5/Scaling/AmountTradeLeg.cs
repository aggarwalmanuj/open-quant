using System;

using NLog;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Scaling
{
    public class AmountTradeLeg : BaseTradeLeg
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public double AmountToFill { get; set; }

        public AmountTradeLeg(int idx, Instrument instrument, OrderSide orderSide, double amt)
            : base(idx, instrument, orderSide)
        {
            this.AmountToFill = amt;
            this.LegName = string.Format("A{0}.{1}.{2}.{3:c}",
                this.Index,
                this.Instrument.Symbol.ToUpper(),
                this.OrderSide,
                this.AmountToFill);

            Log.WriteInfoLog(LegName,
               this.Instrument,
               string.Format("Amount leg generated to {0} {1:c2}", this.OrderSide.ToString().ToUpper(), this.AmountToFill));
        }

        protected override void AdjustPostOrderProcessing(Order order)
        {
            if (IsLegComplete)
                return;

            double temp = FilledAmount - AmountToFill;
            if (temp < order.AvgPrice)
            {
                Log.WriteDebugLog(
                    LegName, 
                    this.Instrument, 
                    string.Format("The remainder amount is {0:c2} which is less than the price {1:c2}. So finalizing ther leg", temp, order.AvgPrice));

                FilledAmount = AmountToFill;
            }
        }

        public override bool IsLegComplete
        {
            get { return FilledAmount >= AmountToFill; }
        }

        protected override string GetLegNamePrefix()
        {
            return "A";
        }

        public override double GetQuantityForNewOrder(double price)
        {
            return (AmountToFill - FilledAmount)/price;
        }


        public override double GetQuantityForExitingOrder(double price)
        {
            var amountRemaining = OrderTracker.GetAmountRemaining();
            double qtyRequired = amountRemaining/price;
            return Convert.ToInt32(Math.Floor(OrderTracker.FilledQuantity + qtyRequired));
        }
    }
}