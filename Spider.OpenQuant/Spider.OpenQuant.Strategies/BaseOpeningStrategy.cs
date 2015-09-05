using System;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseOpeningStrategy : BaseStrategy
    {
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
        public double StopPercentage = 5;
  

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

        [Parameter("Order Side", "Order Management")]
        public OrderSide OrderSide = OrderSide.Buy;

        [Parameter("Round Lots", "Order Management")]
        public bool RoundLots = false;


        #endregion


        /// <summary>
        /// 
        /// </summary>
        protected virtual void HandleStrategyStart()
        {
            InitializeStrategy();

            if (triggerTime.HasValue)
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

            if (IsItTimeToTrigger(bar, true))
            {
                PriceCalculator priceCalc = new PriceCalculator(LoggingConfig);
                QuantityCalculator qtyCalc = new QuantityCalculator(LoggingConfig);

                double targetPrice = priceCalc.Calculate(new PriceCalculatorInput()
                                                                   {
                                                                       CurrentBar = Bar,
                                                                       PreviousBar = GetPreviousBar(Instrument, bar, PeriodConstants.PERIOD_MINUTE),
                                                                       Atr = GetAtrValue(Instrument, AtrPeriod, triggerTime.Value),
                                                                       AllowedSlippage = AllowedSlippage,
                                                                       FavorableGap = FavorableGap,
                                                                       FavorableGapAllowedSlippage = FavorableGapAllowedSlippage,
                                                                       UnfavorableGap = UnfavorableGap,
                                                                       UnfavorableGapAllowedSlippage = UnfavorableGapAllowedSlippage,
                                                                       OrderSide = OrderSide
                                                                   });
                    
                


                double targetQuantity = qtyCalc.Calculate(new QuantityCalculatorInput()
                                                                       {
                                                                           NumberOfPositions = NumberOfPositions,
                                                                           PortfolioAmt = PortfolioAmount,
                                                                           PortfolioAllocationPercentage = PortfolioAllocationPercentage,
                                                                           MaxPortfolioRisk = MaxPortfolioRisk,
                                                                           MaxPositionRisk = MaxPositionRisk,
                                                                           RoundLots = RoundLots,
                                                                           TargetPrice = targetPrice,
                                                                           StopPercentage = StopPercentage,
                                                                           PositionSizePercentage = PositionSizePercentage
                                                                       });

                targetPrice = priceCalc.RoundPrice(targetPrice, Instrument);

                if (targetPrice <= 0 || targetQuantity <= 0)
                    throw new ApplicationException(string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice, targetQuantity));

                string orderName = GetAutoPlacedOrderName(OrderSide, "Auto-Opened", Instrument.Symbol);

                strategyOrder = CreateOrder(OrderSide, targetQuantity, orderName, targetPrice);

                LoggingUtility.LogOrder(LoggingConfig, orderName, OrderSide, targetQuantity, targetPrice, retryCount);

                if (AutoSubmit)
                    strategyOrder.Send();
            }
        }


    }
}
