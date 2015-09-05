using System;
using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.Calculations.StopPriceCalc
{
    public class ProtectiveStopBasedOnAtrCalculator : BaseStopPriceCalculator
    {
        public ProtectiveStopBasedOnAtrCalculator(BaseStrategy strategy)
            : base(strategy)
        {

        }

        public override object Calculate(object param)
        {
            ValidateStrategyType(Enums.StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr);

            OrderSide orderSide = Strategy.EffectiveOrderSide;
            double atrValue = Strategy.EffectiveAtrPrice;
            double atrCoeff = Strategy.CalculatedStopAtrCoefficient;

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
                    returnValue = effectiveLoPrice - (atrValue*atrCoeff);
                    break;

               
                default:
                    throw new NotImplementedException();

            }



            SetIntradayStopPrice(returnValue, effectiveHiPrice, effectiveLoPrice, atrCoeff, (atrValue * atrCoeff));


            return null;
        }

        public override bool NeedToUpdateCurrentDayHiLoPrice()
        {
            return false;
        }

        
    }
}