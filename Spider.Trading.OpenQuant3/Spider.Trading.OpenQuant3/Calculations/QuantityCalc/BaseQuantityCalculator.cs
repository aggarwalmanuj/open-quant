using System;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations.QuantityCalc
{
    public abstract class BaseQuantityCalculator : BaseStrategyCalculator
    {
        public BaseQuantityCalculator(BaseStrategy strategy)
            : base(strategy)
        { }

        protected void ValidateStrategyType(OpeningQuantityCalculatorInput input, PositionSizingCalculationStrategy stratType)
        {
            if (input.PositionSizingCalculationStrategy != stratType)
                throw new ArgumentException("PositionSizingCalculationStrategy", "PositionSizingCalculationStrategy");
        }


        public override object Calculate(object param)
        {
            object returnVal = null;

            if (null != param)
            {

                if (param is OpeningQuantityCalculatorInput)
                    returnVal = CalculateImpl(param as OpeningQuantityCalculatorInput);

                if (param is ClosingQuantityCalculatorInput)
                    returnVal = CalculateImpl(param as ClosingQuantityCalculatorInput);

            }
            else
            {
                throw new ArgumentNullException("param");
            }

            return returnVal;
        }

        protected object CalculateImpl(ClosingQuantityCalculatorInput input)
        {
            return GetFinalQuantity(input.OpenQuantity,
                                    input.RoundLots,
                                    input.PositionSizePercentage,
                                    input.MinimumPosition);

        }

        protected abstract object CalculateImpl(OpeningQuantityCalculatorInput input);

        protected double GetFinalQuantity(double originalQty, bool roundLots, double posSizePercent, double minPos)
        {
            double qty = originalQty;

            qty = qty*posSizePercent/100;
            qty = Math.Round(qty, 0);
            if (roundLots)
                qty = qty - (qty%100);

            LoggingUtility.WriteInfo(
                Strategy.LoggingConfig,
                string.Format(
                    "Total quantity: {0:n2} with a position size of {1:p} = {2}",
                    originalQty,
                    posSizePercent/100,
                    qty));


            if (qty < minPos)
                qty = minPos;

            return qty;
        }

    }
}