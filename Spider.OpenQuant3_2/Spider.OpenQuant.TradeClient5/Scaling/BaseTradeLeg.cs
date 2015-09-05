using System;

using NLog;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Base;
using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Scaling
{
    public abstract class BaseTradeLeg
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public Instrument Instrument { get; set; }

        public OrderSide OrderSide { get; set; }

        protected double FilledAmount { get; set; }

        protected double FilledQuantity { get; set; }

        public DateTime? InitiatedAt { get; set; }

        public DateTime? EndedAt { get; set; }

        public int Index { get; set; }

        public string LegName { get; protected set; }

        protected OrderTracker OrderTracker { get; set; }

        public Logger GetLogger()
        {
            return Log;
        }

        protected abstract string GetLegNamePrefix();

        public virtual string GetEntrySignalName(BaseStrategy strategy)
        {
            return string.Format("{0}{1}.{2}.{3}.{4}.{5}", GetLegNamePrefix(),
                Index,
                strategy.ProjectName,
                strategy.GetCurrentDateTime().ToString("ddMMM.HHmmss"),
                this.OrderSide.ToString().ToUpper(),
                this.Instrument.Symbol);
        }

        public BaseTradeLeg(int idx, Instrument instrument, OrderSide orderSide)
        {
            this.Index = idx;
            this.Instrument = instrument;
            this.OrderSide = orderSide;
        }

        public abstract bool IsLegComplete { get; }

        public abstract double GetQuantityForNewOrder(double price);

        public abstract double GetQuantityForExitingOrder(double price);

        protected virtual void AdjustPostOrderProcessing(Order order)
        {
            
        }

        public virtual void TrackNewOrder(Order order)
        {
            this.OrderTracker = new OrderTracker(order);
        }

        public virtual void OnOrderStatusChanged(Order order)
        {
            if (order.IsDone || order.IsCancelled || order.IsRejected || order.IsFilled)
            {
                FilledAmount = FilledAmount + (order.CumQty*order.AvgPrice);

                // In case fraction of order was left which is less than one share:
                AdjustPostOrderProcessing(order);

                FilledQuantity = FilledQuantity + order.CumQty;

                Log.WriteInfoLog(LegName,
                    this.Instrument,
                    string.Format("OnOrderStatusChanged: FilledAmount={0:c2}, FilledQuantity={1:N2}, IsLegCompleted={2}", FilledAmount, FilledQuantity, IsLegComplete));
            }

            this.OrderTracker.OnOrderStatusChanged(order);
        }
    }
}
