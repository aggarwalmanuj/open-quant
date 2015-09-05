using System;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseClosingStrategy : BaseStrategy
    {

        

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

        protected double? longPositionQuantity = null;
        protected double? shortPositionQuantity = null;

        /// <summary>
        /// 
        /// </summary>
        protected virtual void HandleStrategyStart()
        {
            QueueUpClosingOrder();
        }


        protected void QueueUpClosingOrder()
        {
            string symbol = Instrument.Symbol;
            BrokerAccount IBaccount = DataManager.GetBrokerInfo("IB").Accounts[0];

            foreach (BrokerPosition currentBrokerPosition in IBaccount.Positions)
            {
                if (currentBrokerPosition.Symbol == symbol &&
                    currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                {

                    InitializeStrategy();


                    double? specifiedQty = PositionSizeManager.GetPositionSize(symbol);

                    if (specifiedQty.HasValue)
                    {
                        if (specifiedQty.Value > 0)
                        {
                            longPositionQuantity = specifiedQty.Value;
                            shortPositionQuantity = 0;
                        }
                        else
                        {
                            shortPositionQuantity = Math.Abs(specifiedQty.Value);
                            longPositionQuantity = 0;
                        }
                    }
                    else
                    {
                        longPositionQuantity = currentBrokerPosition.LongQty;
                        shortPositionQuantity = currentBrokerPosition.ShortQty;
                    }

                    PreLoadBarData(Instrument);

                    ATR atr = GetAtr(Instrument, AtrPeriod);

                    if (IsItOkToQueueOrder())
                    {
                        LogOrderQueued();
                        break;
                    }

                }
            }
        }

        protected virtual bool IsItOkToQueueOrder()
        {
            return IsClosingQuantityValid();
        }

        protected bool IsClosingQuantityValid()
        {
            return ((longPositionQuantity.HasValue || shortPositionQuantity.HasValue) && (longPositionQuantity.Value != 0 || shortPositionQuantity.Value != 0));
        }

        protected virtual void LogOrderQueued()
        {
            double origQty = longPositionQuantity > 0 ? Math.Abs(longPositionQuantity.Value) : Math.Abs(shortPositionQuantity.Value) * -1;
            double qty = origQty * PositionSizePercentage / 100;


            LoggingUtility.LogOrderQueued(LoggingConfig, string.Format("Closing for Qty {0:p} x {1:n2} = {2:n2}", PositionSizePercentage / 100, origQty, qty), Instrument.Symbol, triggerTime.Value);
            Console.WriteLine();
        }


        protected PositionSide GetOpenPositionSide()
        {
            if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
            {
                return PositionSide.Long;
            }

            if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
            {
                return PositionSide.Short;
            }

            return PositionSide.Long;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        protected virtual void HandleBarOpen(Bar bar)
        {

            if (RetryOrder(bar, HandleStrategyStart))
            {
                LoggingUtility.LogRetryOrder(LoggingConfig, bar, retryCount);
            }

            if (IsItTimeToTrigger(bar,  true))
            {

                OrderSide? orderSide = null;
                double targetQuantity = 0;
                double targetPrice = 0;

                PriceCalculator priceCalc = new PriceCalculator(LoggingConfig);
                QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);


                if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
                {
                    orderSide = OrderSide.Sell;
                    targetPrice = priceCalc.Calculate(
                        new PriceCalculatorInput()
                            {
                                CurrentBar = Bar,
                                PreviousBar = GetPreviousBar(Instrument, bar, PeriodConstants.PERIOD_MINUTE),
                                Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                AllowedSlippage = AllowedSlippage,
                                FavorableGap = FavorableGap,
                                FavorableGapAllowedSlippage = FavorableGapAllowedSlippage,
                                UnfavorableGap = UnfavorableGap,
                                UnfavorableGapAllowedSlippage = UnfavorableGapAllowedSlippage,
                                OrderSide = orderSide.Value
                            });

                    targetQuantity = qtyCalc.CalculatePositionSizedQuantity(longPositionQuantity.Value,
                                                                                       new QuantityCalculatorInput()
                                                                                           {
                                                                                               PositionSizePercentage = PositionSizePercentage,
                                                                                               RoundLots = RoundLots
                                                                                           });
                }

                if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
                {
                    orderSide = OrderSide.Buy;
                    targetPrice = priceCalc.Calculate(
                        new PriceCalculatorInput()
                            {
                                CurrentBar = Bar,
                                PreviousBar = GetPreviousBar(Instrument, bar, PeriodConstants.PERIOD_MINUTE),
                                Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                AllowedSlippage = AllowedSlippage,
                                FavorableGap = FavorableGap,
                                FavorableGapAllowedSlippage = FavorableGapAllowedSlippage,
                                UnfavorableGap = UnfavorableGap,
                                UnfavorableGapAllowedSlippage = UnfavorableGapAllowedSlippage,
                                OrderSide = orderSide.Value
                            });

                    targetQuantity = qtyCalc.CalculatePositionSizedQuantity(shortPositionQuantity.Value,
                                                                                       new QuantityCalculatorInput()
                                                                                           {
                                                                                               PositionSizePercentage = PositionSizePercentage,
                                                                                               RoundLots = RoundLots
                                                                                           });
                }

                targetPrice = priceCalc.RoundPrice(targetPrice, Instrument);

                if (targetPrice <= 0 || targetQuantity <= 0)
                    throw new ApplicationException(string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice, targetQuantity));


                if (orderSide.HasValue)
                {
                    string orderName = GetAutoPlacedOrderName(orderSide.Value, "Auto-Closed", Instrument.Symbol);


                    strategyOrder = CreateOrder(orderSide.Value, targetQuantity, orderName, targetPrice);

                    LoggingUtility.LogOrder(LoggingConfig, orderName, orderSide.Value, targetQuantity, targetPrice, retryCount);

                    if (AutoSubmit)
                        strategyOrder.Send();
                }
            }
        }
    }
}