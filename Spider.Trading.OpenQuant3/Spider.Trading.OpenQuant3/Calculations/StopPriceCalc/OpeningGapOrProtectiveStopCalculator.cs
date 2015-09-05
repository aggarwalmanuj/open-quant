namespace Spider.Trading.OpenQuant3.Calculations.StopPriceCalc
{
    public class OpeningGapOrProtectiveStopCalculator : BaseStopPriceCalculator
    {
        private BaseStopPriceCalculator openGapCalc;
        private BaseStopPriceCalculator intraStopCalc;

        public OpeningGapOrProtectiveStopCalculator(BaseStrategy strategy)
            : base(strategy)
        {
            openGapCalc = new OpeningGapStopCalculator(strategy);
            intraStopCalc = new ProtectiveStopBasedOnAtrCalculator(strategy);
        }

        public override object Calculate(object param)
        {
            ValidateStrategyType(Enums.StopPriceCalculationStrategy.OpeningGapOrProtectiveStop);

            openGapCalc.Calculate(param);
            intraStopCalc.Calculate(param);

            return null;
        }


        public override bool NeedToUpdateCurrentDayHiLoPrice()
        {
            return false;
        }


    }
}