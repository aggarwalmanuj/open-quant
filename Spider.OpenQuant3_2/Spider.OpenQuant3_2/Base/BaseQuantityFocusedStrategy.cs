using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.OpenQuant3_2.Util;

namespace Spider.OpenQuant3_2.Base
{
    public abstract class BaseQuantityFocusedStrategy : BaseStrategy
    {
        protected int? InitialOpenQuantity { get; set; }
        protected int? InitialQuantityToBeFill { get; set; }
        protected int FilledQuantity { get; set; }
        protected int RemainingQuantity { get; set; }
        protected PositionSide? OpenPositionSide { get; set; }


        #region Position Sizing Management

        [Parameter("Specific Position Size", "Position Sizing Management")] public int? SpecificPositionSize;

        [Parameter("Specific Position Side To Reverse", "Position Sizing Management")] public PositionSide? SpecificPositionSideToReverse;

        #endregion

        public override void OnStrategyStart()
        {
            if (SpecificPositionSize.HasValue && SpecificPositionSideToReverse.HasValue && SpecificPositionSize.Value != 0)
            {

                OpenPositionSide = SpecificPositionSideToReverse.Value;
                InitialOpenQuantity = Convert.ToInt32(Math.Abs(SpecificPositionSize.Value));
                LoggingUtility.WriteHorizontalBreak(this);
                LoggingUtility.WriteInfo(this,
                    string.Format("***  Found a SPECIFIC QTY {0} position of {1} shares for {2} in account {3} ***",
                        OpenPositionSide.Value.ToString().ToUpper(),
                        InitialOpenQuantity,
                        this.Instrument.Symbol.ToUpper(),
                        this.IbAccountNumber));
                LoggingUtility.WriteHorizontalBreak(this);

                InitialQuantityToBeFill = InitialOpenQuantity;
            }
            else
            {
                InferOpenQuantityFromBroker();
            }

            base.OnStrategyStart();
        }

        protected void InferOpenQuantityFromBroker()
        {

            double? LongPositionQuantity = null;
            double? ShortPositionQuantity = null;
            string symbol = Instrument.Symbol;
            BrokerAccount ibAccount = DataManager.GetBrokerInfo("IB").Accounts[IbAccountNumber];

            foreach (BrokerPosition currentBrokerPosition in ibAccount.Positions)
            {
                string posSymbol = currentBrokerPosition.Symbol.Trim().Replace(" ", "");
                bool isSymbolSame = string.Compare(symbol, posSymbol, StringComparison.InvariantCultureIgnoreCase) == 0;

                if (isSymbolSame && currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                {
                    LongPositionQuantity = currentBrokerPosition.LongQty;
                    ShortPositionQuantity = currentBrokerPosition.ShortQty;

                    if (LongPositionQuantity.HasValue && LongPositionQuantity.Value > 0)
                    {
                        OpenPositionSide = PositionSide.Long;
                        InitialOpenQuantity = Convert.ToInt32(Math.Abs(LongPositionQuantity.Value));
                    }
                    else if (ShortPositionQuantity.HasValue && ShortPositionQuantity.Value > 0)
                    {
                        OpenPositionSide = PositionSide.Short;
                        InitialOpenQuantity = Convert.ToInt32(Math.Abs(ShortPositionQuantity.Value));
                    }
                    else
                    {
                        throw new InvalidOperationException(
                            string.Format("Could not retrieve an open position for {0} in account {1}",
                                this.Instrument.Symbol,
                                this.IbAccountNumber));
                    }

                    LoggingUtility.WriteHorizontalBreak(this);
                    LoggingUtility.WriteInfo(this,
                        string.Format("***  Found an OPEN {0} position of {1} shares for {2} in account {3} ***",
                            OpenPositionSide.Value.ToString().ToUpper(),
                            InitialOpenQuantity,
                            this.Instrument.Symbol.ToUpper(),
                            this.IbAccountNumber));
                    LoggingUtility.WriteHorizontalBreak(this);


                    InitialQuantityToBeFill =
                        Convert.ToInt32(Math.Floor(InitialOpenQuantity.Value*PositionSizePercentage/100d));

                    break;
                }
            }
        }


        protected override void HandlePartiallyFilledQuantity(OpenQuant.API.Order order)
        {
            LoggingUtility.WriteInfoFormat(this, "ORDER PARTIALLY FILLED: Filled Qty={0}, Avg. Fill Price={1:c}",
                order.CumQty, order.AvgPrice);

            RemainingQuantity = Convert.ToInt32(order.LeavesQty);

            FilledQuantity = Convert.ToInt32(order.CumQty);
        }



        protected override int GetBuyQuantity()
        {
            if (InitialOpenQuantity == null || OpenPositionSide == null)
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

            return InitialQuantityToBeFill.Value;
        }

        protected override int GetSellQuantity()
        {
            if (InitialOpenQuantity == null || OpenPositionSide == null)
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

            return InitialQuantityToBeFill.Value;
        }

        public override OpenQuant.API.OrderSide GetOrderSide()
        {
            if (InitialOpenQuantity == null || OpenPositionSide == null)
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }

            if (OpenPositionSide.Value == PositionSide.Long)
            {
                return OrderSide.Sell;
            }
            else if (OpenPositionSide.Value == PositionSide.Short)
            {
                return OrderSide.Buy;
            }
            else
            {
                throw new InvalidOperationException(
                    string.Format("Could not retrieve an open position for {0} in account {1}", this.Instrument.Symbol,
                        this.IbAccountNumber));
            }
            
        }
    }
}
