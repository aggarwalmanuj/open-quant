using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.Strategies.Entities;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseFlipFlopStrategy : BaseStrategy
    {
        #region FlipFlop Management

        [Parameter("Open Quantity", "FlipFlop Management")]
        public double OpenQuantity = 100;

        [Parameter("Flip Flop Pair", "FlipFlop Management")]
        public FlipFlopPair FlipFlopPair = FlipFlopPair.None;

        [Parameter("Flip Flop Position Side", "FlipFlop Management")]
        public PositionSide PositionSide = PositionSide.Long;

        [Parameter("Portfolio Amount Includes Open Position", "FlipFlop Management")]
        public bool AmountIncludesOpenPosition = true;

        #endregion



        #region Money Management

        [Parameter("Portfolio Amount", "Money Management")]
        public double PortfolioAmount = 10000;

        [Parameter("Portfolio Allocation Percentage", "Money Management")]
        public double PortfolioAllocationPercentage = 33;

        [Parameter("Portfolio Positions", "Money Management")]
        public int NumberOfPositions = 5;

        [Parameter("Max Portfolio Risk", "Money Management")]
        public double MaxPortfolioRisk = 6;

        [Parameter("Max Position Risk", "Money Management")]
        public double MaxPositionRisk = 2;

        [Parameter("Stop", "Money Management")]
        public double StopPercentage = 8;


        #endregion

        #region Slippage Management

        [Parameter("Atr Period", "Slippage Management")]
        public int AtrPeriod = 14;

        [Parameter("Allowed Slippage", "Slippage Management")]
        public double AllowedSlippage = 0.25;

        #endregion

        #region Gap Management

        [Parameter("Favorable Gap", "Gap Management")]
        public double FavorableGap = 1;

        [Parameter("Favorable Gap Allowed Slippage", "Gap Management")]
        public double FavorableGapAllowedSlippage = 0.15;


        [Parameter("Unfavorable Gap", "Gap Management")]
        public double UnfavorableGap = 1.25;

        [Parameter("Unfavorable Gap Allowed Slippage", "Gap Management")]
        public double UnfavorableGapAllowedSlippage = 0.15;

        #endregion

        #region Order Management

        [Parameter("Round Lots", "Order Management")]
        public bool RoundLots = false;


        #endregion


        private OrderSide OrderSide = OrderSide.Buy;
        private bool closingOrderQueued = false;
        private string longSymbol = null;
        private string shortSymbol = null;
        private string openingSymbol = null;
        private string closingSymbol = null;
        private Instrument closingInstrument = null;
        private Instrument openingInstrument = null;
        private Order closingOrder = null;

        /// <summary>
        /// 
        /// </summary>
        protected virtual void HandleStrategyStart()
        {
            InitializeStrategy();

            PreProcessStrategy();

            if (string.IsNullOrEmpty(closingSymbol) || OpenQuantity <= 0)
            {
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Flipping {0}. Buying {1}", PositionSide,
                                                       openingSymbol));
            }
            else
            {
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Flipping {0}. Buying {1}, Selling {2}", PositionSide,
                                                       openingSymbol, closingSymbol));
            }

            if (Instrument.Symbol == openingSymbol)
            {
                QueueClosingOrder();

                QueueOpeningOrder();
            }
        }

        private void PreProcessStrategy()
        {
            FlipFlopPairDescriptionAttribute att = FlipFlopPairDescriptionAttribute.GetAttribute(FlipFlopPair);
            if (null == att)
                throw new ArgumentNullException("Flipflop Description Attribute", "This is not a valid flip flop pair");

            longSymbol = att.LongSymbol;
            shortSymbol = att.ShortSymbol;

            if (PositionSide == PositionSide.Long)
            {
                closingSymbol = shortSymbol;
                openingSymbol = longSymbol;
            }
            else
            {
                closingSymbol = longSymbol;
                openingSymbol = shortSymbol;
            }


            openingInstrument = InstrumentManager.Instruments[openingSymbol];

            if (!string.IsNullOrEmpty(closingSymbol))
            {
                closingInstrument = InstrumentManager.Instruments[closingSymbol];
            }
        }

        private void QueueClosingOrder()
        {
            if (OpenQuantity <= 0)
                return;

            if (openingSymbol != Instrument.Symbol)
                return;


            try
            {
                LoggingConfig.Instrument = closingInstrument;

                BrokerAccount IBaccount = DataManager.GetBrokerInfo("IB").Accounts[0];

                bool foundPosition = false;

                foreach (BrokerPosition currentBrokerPosition in IBaccount.Positions)
                {
                    if (currentBrokerPosition.Symbol == closingSymbol &&
                        currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                    {
                        double? specifiedQty = currentBrokerPosition.LongQty;

                        if (specifiedQty.HasValue)
                        {
                            if (specifiedQty.Value >= OpenQuantity)
                            {
                                foundPosition = true;
                                break;
                            }
                        }
                    }
                }

                if (foundPosition)
                {
                    PreLoadBarData(closingInstrument);

                    ATR atr = GetAtr(closingInstrument, AtrPeriod);

                    closingOrderQueued = foundPosition;

                    LoggingUtility.LogOrderQueued(LoggingConfig, "Closing", closingSymbol, triggerTime.Value);
                }
                else
                {
                    if (OpenQuantity > 0)
                    {
                        throw new ApplicationException(
                            string.Format(
                                "No open positions for {0} found greater than equal to specifid open quantity",
                                closingSymbol));
                    }
                }
            }
            finally
            {
                LoggingConfig.Instrument = Instrument;
            }
        }

        private void QueueOpeningOrder()
        {
            if (triggerTime.HasValue && Instrument.Symbol == openingSymbol)
            {
                LoggingUtility.LogOrderQueued(LoggingConfig, "Opening", Instrument.Symbol, triggerTime.Value);

                try
                {

                    PreLoadBarData(Instrument);

                    ATR atr = GetAtr(Instrument, AtrPeriod);

                }
                catch (Exception ex)
                {
                    LoggingUtility.WriteError(LoggingConfig, string.Format("Error in Opening Order: {0}", ex));
                    triggerTime = null;
                }

            }
        }




        protected virtual void HandleBarSlice(long size)
        {
            if (IsItTimeToTrigger(Instrument.Bar, true) && Instrument.Symbol == openingSymbol)
            {
                try
                {
                    LoggingConfig.Instrument = closingInstrument;

                    ExecuteClosingOrder(size);
                }
                finally
                {
                    LoggingConfig.Instrument = Instrument;
                }

                ExecuteOpeningOrder(size);
            }
        }

        private void ExecuteClosingOrder(long size)
        {
            if (OpenQuantity > 0 && closingOrderQueued)
            {
                OrderSide orderSide = OrderSide.Sell;
                double targetQuantity = 0;
                double targetPrice = 0;


                PriceCalculator priceCalc = new PriceCalculator(LoggingConfig);
                QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);


                targetPrice = priceCalc.Calculate(
                    new PriceCalculatorInput()
                        {
                            CurrentBar = closingInstrument.Bar,
                            PreviousBar = GetPreviousBar(closingInstrument, closingInstrument.Bar, PeriodConstants.PERIOD_MINUTE),
                            Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                            AllowedSlippage = AllowedSlippage,
                            FavorableGap = FavorableGap,
                            FavorableGapAllowedSlippage = FavorableGapAllowedSlippage,
                            UnfavorableGap = UnfavorableGap,
                            UnfavorableGapAllowedSlippage = UnfavorableGapAllowedSlippage,
                            OrderSide = orderSide
                        });

                targetQuantity = qtyCalc.CalculatePositionSizedQuantity(OpenQuantity,
                                                                        new QuantityCalculatorInput()
                                                                            {
                                                                                PositionSizePercentage = PositionSizePercentage,
                                                                                RoundLots = RoundLots
                                                                            });



                targetPrice = priceCalc.RoundPrice(targetPrice, closingInstrument);

                if (targetPrice <= 0 || targetQuantity <= 0)
                    throw new ApplicationException(
                        string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice,
                                      targetQuantity));


                string orderName = GetAutoPlacedOrderName(orderSide, "FlipFlop-Closed", closingInstrument.Symbol);

                closingOrder = CreateOrder(orderSide, targetQuantity, orderName, targetPrice);

                LoggingUtility.LogOrder(LoggingConfig, orderName, orderSide, targetQuantity, targetPrice, retryCount);

                if (AutoSubmit)
                    closingOrder.Send();


                if (!AmountIncludesOpenPosition)
                {
                    double proceeds = (targetPrice * targetQuantity);
                    PortfolioAmount = PortfolioAmount + proceeds;
                    LoggingUtility.WriteInfo(LoggingConfig,
                                             string.Format(
                                                 "The sale of {0} for {1:c} have been added to portfolio. New total = {2:c} ",
                                                 closingInstrument.Symbol,
                                                 proceeds, PortfolioAmount));
                }
            }
        }


        private void ExecuteOpeningOrder(long size)
        {

            PriceCalculator priceCalc = new PriceCalculator(LoggingConfig);
            QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);

            double targetPrice = priceCalc.Calculate(new PriceCalculatorInput()
                                                         {
                                                             CurrentBar = openingInstrument.Bar,
                                                             PreviousBar =
                                                                 GetPreviousBar(openingInstrument, openingInstrument.Bar,
                                                                                PeriodConstants.PERIOD_MINUTE),
                                                             Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                                             AllowedSlippage = AllowedSlippage,
                                                             FavorableGap = FavorableGap,
                                                             FavorableGapAllowedSlippage = FavorableGapAllowedSlippage,
                                                             UnfavorableGap = UnfavorableGap,
                                                             UnfavorableGapAllowedSlippage =
                                                                 UnfavorableGapAllowedSlippage,
                                                             OrderSide = OrderSide
                                                         });




            double targetQuantity = qtyCalc.Calculate(new QuantityCalculatorInput()
                                                          {
                                                              NumberOfPositions = NumberOfPositions,
                                                              PortfolioAmt = PortfolioAmount,
                                                              PortfolioAllocationPercentage =
                                                                  PortfolioAllocationPercentage,
                                                              MaxPortfolioRisk = MaxPortfolioRisk,
                                                              MaxPositionRisk = MaxPositionRisk,
                                                              RoundLots = RoundLots,
                                                              TargetPrice = targetPrice,
                                                              StopPercentage = StopPercentage,
                                                              PositionSizePercentage = PositionSizePercentage
                                                          });

            targetPrice = priceCalc.RoundPrice(targetPrice, Instrument);

            if (targetPrice <= 0 || targetQuantity <= 0)
                throw new ApplicationException(
                    string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice,
                                  targetQuantity));

            string orderName = GetAutoPlacedOrderName(OrderSide, "FlipFlop-Opened", Instrument.Symbol);

            strategyOrder = CreateOrder(OrderSide, targetQuantity, orderName, targetPrice);

            LoggingUtility.LogOrder(LoggingConfig, orderName, OrderSide, targetQuantity, targetPrice, retryCount);

            if (AutoSubmit)
                strategyOrder.Send();

        }


        protected override bool IsItTimeToTrigger(Bar bar, bool logBar)
        {


            if (Instrument.Symbol != openingSymbol)
                return false;

            bool returnValue = false;


            try
            {
                if (closingOrderQueued)
                    returnValue = (triggerTime.HasValue) && (bar.BeginTime >= triggerTime) && (!HasPosition && strategyOrder == null && closingOrder == null);
                else
                    returnValue = (triggerTime.HasValue) && (bar.BeginTime >= triggerTime) && (!HasPosition && strategyOrder == null);
            }
            finally
            {
                if (returnValue && logBar)
                    LoggingUtility.LogOkToTriggerOrder(LoggingConfig, bar);
            }


            return returnValue;
        }
    }

}
