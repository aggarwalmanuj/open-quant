using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseStopOpeningStrategy : BaseOpeningStrategy
    {

        protected double? stopPrice = null;


        /// <summary>
        /// 
        /// </summary>
        protected override void HandleStrategyStart()
        {
            InitializeStrategy();


            if (triggerTime.HasValue)
            {
                stopPrice = StopPriceManager.GetStopPrice(Instrument.Symbol);

                if (stopPrice.HasValue)
                {
                    LoggingUtility.LogStopOrderQueued(LoggingConfig, "Opening", Instrument.Symbol, triggerTime.Value, stopPrice.Value);

                    PreLoadBarData(Instrument);

                    ATR atr = GetAtr(Instrument, AtrPeriod);
                }
                else
                {
                    LoggingUtility.LogNoStopOrderFound(LoggingConfig);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        protected override void HandleBarOpen(Bar bar)
        {
            if (IsItTimeToTrigger(bar, false))
            {
                if (stopPrice.HasValue)
                {
                    double lastClosePrice = bar.Close;

                    if (StopPriceManager.IsEntryStopPriceMet(lastClosePrice, stopPrice.Value, OrderSide))
                    {
                        LoggingUtility.LogCurrentBarArrival(LoggingConfig, bar);

                        PriceCalculator priceCalc = new PriceCalculator(LoggingConfig);
                        double targetPrice = priceCalc.CalculateSlippageAdjustedPrice(new PriceCalculatorInput()
                                                                                                {
                                                                                                    AllowedSlippage = AllowedSlippage,
                                                                                                    Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                                                                                    CurrentBar = bar,
                                                                                                    PreviousBar = GetPreviousBar(Instrument, bar, PeriodConstants.PERIOD_MINUTE),
                                                                                                    OrderSide = OrderSide
                                                                                                });

                        double targetQuantity = new QuantityCalculator(LoggingConfig).Calculate(new QuantityCalculatorInput()
                                                                                 {
                                                                                     MaxPortfolioRisk = MaxPortfolioRisk,
                                                                                     MaxPositionRisk = MaxPositionRisk,
                                                                                     NumberOfPositions = NumberOfPositions,
                                                                                     PortfolioAmt = PortfolioAmount,
                                                                                     PortfolioAllocationPercentage = PortfolioAllocationPercentage,
                                                                                     PositionSizePercentage = PositionSizePercentage,
                                                                                     RoundLots = RoundLots,
                                                                                     StopPercentage = StopPercentage,
                                                                                     TargetPrice = targetPrice
                                                                                 });


                        targetPrice = priceCalc.RoundPrice(targetPrice, Instrument);

                        if (targetPrice <= 0 || targetQuantity <= 0)
                            throw new ApplicationException(string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice, targetQuantity));

                        string orderName = GetAutoPlacedOrderName(OrderSide, string.Format("Auto-Open Stop Order @ {0:c}", stopPrice.Value), Instrument.Symbol);

                        strategyOrder = CreateOrder(OrderSide, targetQuantity, orderName, targetPrice);

                        LoggingUtility.LogOrder(LoggingConfig, orderName, OrderSide, targetQuantity, targetPrice, retryCount);

                        if (AutoSubmit)
                            strategyOrder.Send();
                    }
                }

            }

        }
    }
}
