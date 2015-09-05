using System;
using Spider.OpenQuant4.Base;
using Spider.OpenQuant4.Util;

namespace Spider.OpenQuant4.Opening
{
    public abstract partial class BaseOpeningStrategy : BaseStrategy
    {

        protected double? InitialPositionAmount { get; set; }

        protected double FilledAmount { get; set; }

        protected int FilledQuantity { get; set; }

        public override void OnStrategyStart()
        {
            InitialPositionAmount = ((TotalPortfolioAmount / NumberOfPortfolioPositions) * PositionSizePercentage / 100d);

            LoggingUtility.WriteHorizontalBreak(this);
            LoggingUtility.WriteInfo(this,
                string.Format("*** Calculated OPENING position size amount={0:c} to {1} for {2} in account {3} ***",
                    InitialPositionAmount,
                    GetOrderSide().ToString().ToUpper(),
                    this.Instrument.Symbol.ToUpper(),
                    this.IbAccountNumber));
            LoggingUtility.WriteHorizontalBreak(this);



            base.OnStrategyStart();
        }

        protected override int GetBuyQuantity()
        {
            var amountRemaining = GetAmountRemaining();
            double qtyRequired = amountRemaining / GetBuyPrice();
            return Convert.ToInt32(Math.Floor(FilledQuantity + qtyRequired));
        }

        protected override int GetSellQuantity()
        {
            var amountRemaining = GetAmountRemaining();
            double qtyRequired = amountRemaining / GetSellPrice();
            return Convert.ToInt32(Math.Floor(FilledQuantity + qtyRequired));
        }



        public override OpenQuant.API.OrderSide GetOrderSide()
        {
            return OpeningOrderSide;
        }

        protected override void HandlePartiallyFilledQuantity(OpenQuant.API.Order order)
        {
            LoggingUtility.WriteInfoFormat(this, "ORDER PARTIALLY FILLED: Filled Qty={0}, Avg. Fill Price={1:c}",
                order.CumQty, order.AvgPrice);
            FilledAmount = (order.CumQty * order.AvgPrice);
            FilledQuantity = Convert.ToInt32(order.CumQty);
            if (FilledAmount > InitialPositionAmount.Value)
            {
                LoggingUtility.WriteHorizontalBreak(this);
                LoggingUtility.WriteInfo(this, " ORDER OVERFILLED");
                LoggingUtility.WriteHorizontalBreak(this);
            }
        }


        protected double GetAmountRemaining()
        {
            if (InitialPositionAmount == null)
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an opening amount for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

            double amountRemaining = InitialPositionAmount.Value - FilledAmount;
            LoggingUtility.WriteInfoFormat(this, "Amount remaining to open: {0:c}", amountRemaining);
            return amountRemaining;
        }
    }
}