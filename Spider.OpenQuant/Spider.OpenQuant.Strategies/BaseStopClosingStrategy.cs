using System;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseStopClosingStrategy : BaseClosingStrategy
    {
        protected double? stopPrice = null;

        /// <summary>
        /// 
        /// </summary>
        protected override void HandleStrategyStart()
        {
            QueueUpClosingOrder();
        }

        protected override bool IsItOkToQueueOrder()
        {
            stopPrice = StopPriceManager.GetStopPrice(Instrument.Symbol);
            bool stopPriceOk = stopPrice.HasValue;

            if (!stopPriceOk)
                LoggingUtility.LogNoStopOrderFound(LoggingConfig);

            return stopPriceOk && IsClosingQuantityValid();
        }

        protected override void LogOrderQueued()
        {
            double origQty = longPositionQuantity > 0 ? Math.Abs(longPositionQuantity.Value) : Math.Abs(shortPositionQuantity.Value) * -1;
            double qty = origQty * PositionSizePercentage / 100;


            LoggingUtility.LogOrderQueued(LoggingConfig, string.Format("Closing for Qty {0:p} x {1:n2} = {2:n2} @ stop price {3:c}", PositionSizePercentage / 100, origQty, qty, stopPrice.Value), Instrument.Symbol, triggerTime.Value);
        }


        protected override void HandleBarOpen(Bar bar)
        {
            if (IsItTimeToTrigger(bar, false))
            {
                if (stopPrice.HasValue)
                {
                    double lastClosePrice = bar.Close;
                    OrderSide? orderSide = null;
                    PositionSide? positionSide = null;

                    double targetPrice = lastClosePrice;
                    double targetQuantity = 0;

                    if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
                    {
                        orderSide = OrderSide.Sell;
                        positionSide = PositionSide.Long;
                    }

                    if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
                    {
                        orderSide = OrderSide.Buy;
                        positionSide = PositionSide.Short;
                    }

                    if (StopPriceManager.IsExitStopPriceMet(lastClosePrice, stopPrice.Value, positionSide.Value))
                    {
                        LoggingUtility.LogCurrentBarArrival(LoggingConfig, bar);

                        PriceCalculator priceCalc = new PriceCalculator(LoggingConfig);
                        QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);

                        if (positionSide.Value == PositionSide.Long)
                        {
                            targetPrice = priceCalc.CalculateSlippageAdjustedPrice(
                                new PriceCalculatorInput()
                                    {
                                        AllowedSlippage = AllowedSlippage,
                                        Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                        CurrentBar = bar,
                                        OrderSide = orderSide.Value,
                                        PreviousBar = GetPreviousBar(Instrument, bar, PeriodConstants.PERIOD_MINUTE)
                                    });

                            targetQuantity = qtyCalc.CalculatePositionSizedQuantity(
                                longPositionQuantity.Value,
                                new QuantityCalculatorInput()
                                    {
                                        PositionSizePercentage = PositionSizePercentage,
                                        RoundLots = RoundLots
                                    });
                        }

                        if (positionSide.Value == PositionSide.Short)
                        {
                            targetPrice = priceCalc.CalculateSlippageAdjustedPrice(
                                new PriceCalculatorInput()
                                    {
                                        AllowedSlippage = AllowedSlippage,
                                        Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                        CurrentBar = bar,
                                        OrderSide = orderSide.Value,
                                        PreviousBar = GetPreviousBar(Instrument, bar, PeriodConstants.PERIOD_MINUTE)
                                    });

                            targetQuantity = qtyCalc.CalculatePositionSizedQuantity(
                                shortPositionQuantity.Value,
                                new QuantityCalculatorInput()
                                    {
                                        PositionSizePercentage = PositionSizePercentage,
                                        RoundLots = RoundLots
                                    });
                        }

                        targetPrice = priceCalc.RoundPrice(targetPrice, Instrument);

                        if (targetPrice <= 0 || targetQuantity <= 0)
                            throw new ApplicationException(string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice, targetQuantity));

                        string orderName = GetAutoPlacedOrderName(orderSide.Value, string.Format("Auto-Close Stop Order @ {0:c}", stopPrice.Value), Instrument.Symbol);

                        strategyOrder = CreateOrder(orderSide.Value, targetQuantity, orderName, targetPrice);

                        LoggingUtility.LogOrder(LoggingConfig, orderName, orderSide.Value, targetQuantity, targetPrice, retryCount);

                        if (AutoSubmit)
                            strategyOrder.Send();
                    }

                }
            }

        }


    }
}