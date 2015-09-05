using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLog;

using OpenQuant.API;

namespace Spider.OpenQuant.TradeClient5.Scaling
{
    public class OrderTracker
    {
        protected static readonly Logger Log = LogManager.GetCurrentClassLogger();

        private readonly Order Order = null;

        public double QuantityToFill { get; set; }

        protected double OriginalPrice { get; set; }

        protected double AmountToFill { get; set; }

        public double FilledQuantity { get; set; }

        protected double FilledAmount { get; set; }

        private bool IsOrderDone { get; set; }

        private bool IsOrderPartiallyFilled { get; set; }

        private bool IsOrderFilled { get; set; }

        public OrderTracker(Order order)
        {
            Order = order;
            this.QuantityToFill = order.Qty;
            this.OriginalPrice = order.Price;
            this.AmountToFill = this.QuantityToFill*this.OriginalPrice;
            FilledAmount = 0;
            FilledQuantity = 0;
        }


        public virtual void OnOrderStatusChanged(Order order)
        {
            if (string.Compare(this.Order.Text, order.Text, StringComparison.InvariantCultureIgnoreCase) != 0)
                return;

            if (order.Status == OrderStatus.Filled)
            {
                IsOrderFilled = true;
                IsOrderDone = true;
            }
            else if (order.Status == OrderStatus.PartiallyFilled)
            {
                FilledAmount = (order.CumQty * order.AvgPrice);
                FilledQuantity = Convert.ToInt32(order.CumQty);

                IsOrderPartiallyFilled = true;
            }
            else if (order.Status == OrderStatus.Rejected || order.Status == OrderStatus.Cancelled)
            {
                IsOrderDone = true;
            }
            else if (order.IsDone)
            {
                IsOrderDone = true;
            }
        }


        public double GetAmountRemaining()
        {
            if (IsOrderDone || IsOrderFilled)
                return 0;


            double amountRemaining = AmountToFill - FilledAmount;
            return amountRemaining;
        }
    }
}
