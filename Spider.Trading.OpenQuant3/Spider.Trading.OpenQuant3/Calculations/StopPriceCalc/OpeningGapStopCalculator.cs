using System;
using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Calculations.StopPriceCalc
{
    public class OpeningGapStopCalculator : BaseStopPriceCalculator
    {
        public OpeningGapStopCalculator(BaseStrategy strategy)
            : base(strategy)
        {

        }

        public override object Calculate(object param)
        {
            ValidateStrategyType(Enums.StopPriceCalculationStrategy.OpeningGap);

            OrderSide orderSide = Strategy.EffectiveOrderSide;
            double atrValue = Strategy.EffectiveAtrPrice;
            double atrCoeff = Strategy.OpeningGapAtrCoefficient;

            double effectiveLoPrice = GetEffectiveLoPrice();
            double effectiveHiPrice = GetEffectiveHiPrice();


            double returnValue = 0;


            switch (orderSide)
            {
                case OrderSide.Buy:
                    // Protective is to make sure price does not higher than yesterday's high
                    returnValue = effectiveHiPrice + (atrValue * atrCoeff);
                    break;


                case OrderSide.Sell:
                    // Protective is to make sure price does not go lower than yesterday's low
                    returnValue = effectiveLoPrice - (atrValue * atrCoeff);
                    break;


                default:
                    throw new NotImplementedException();

            }



            SetOpeningGapStopPrice(returnValue, effectiveHiPrice, effectiveLoPrice, atrCoeff, (atrValue * atrCoeff));


            return null;
        }

        public override bool NeedToUpdateCurrentDayHiLoPrice()
        {
            return false;
        }

        public override Enums.StopCalculationReferencePriceStrategy GetStopPriceReferenceStrategy()
        {
            return Strategy.OpeningGapReferencePriceStrategy;
        }
    }
}