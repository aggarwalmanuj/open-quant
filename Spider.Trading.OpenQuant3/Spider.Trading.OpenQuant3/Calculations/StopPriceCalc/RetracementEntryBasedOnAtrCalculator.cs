using System;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Calculations.StopPriceCalc
{
    public class RetracementEntryBasedOnAtrCalculator : BaseStopPriceCalculator
    {
        public RetracementEntryBasedOnAtrCalculator(BaseStrategy strategy)
            : base(strategy)
        {

        }

        public override object Calculate(object param)
        {
            ValidateStrategyType(Enums.StopPriceCalculationStrategy.RetracementEntryBasedOnAtr);

            OrderSide orderSide = Strategy.EffectiveOrderSide;
            double atrValue = Strategy.EffectiveAtrPrice;
            double atrCoeff = Strategy.CalculatedStopAtrCoefficient;

            double effectiveLoPrice = GetEffectiveLoPrice();
            double effectiveHiPrice = GetEffectiveHiPrice();

            double returnValue = 0;


            switch (orderSide)
            {
                case OrderSide.Buy:
                    // Retrace from previous Lo to make the entry
                    returnValue = effectiveLoPrice + (atrValue * atrCoeff);
                    break;

                case OrderSide.Sell:
                    // Retrace from previous Hi to make the entry
                    returnValue = effectiveHiPrice - (atrValue * atrCoeff);
                    break;

                
                default:
                    throw new NotImplementedException();

            }

            SetIntradayStopPrice(returnValue, effectiveHiPrice, effectiveLoPrice, atrCoeff, (atrValue * atrCoeff));

            return null;
        }


        public override bool NeedToUpdateCurrentDayHiLoPrice()
        {
            return true;
        }

        
    }
}