using System.Collections.Generic;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public abstract class BaseTemplate
    {
        protected abstract OrderSide OrderSide { get; }
        private Dictionary<string, object> paramsDictionary = ParamsGeneratorUtility.GetGenericParamsDictionary();

        internal Dictionary<string, object> Generate()
        {
            ParamsSetterUtility.SetOrderSide(paramsDictionary, this.OrderSide);
            return paramsDictionary;
        }

        public BaseTemplate AtOpen()
        {
            ParamsSetterUtility.SetOpeningOrder(paramsDictionary);
            return this;
        }

        public BaseTemplate AtClose()
        {
            ParamsSetterUtility.SetMarketOnClose(paramsDictionary);
            return this;
        }

        public BaseTemplate Intraday()
        {
            ParamsSetterUtility.SetIntraday(paramsDictionary);
            return this;
        }

        public BaseTemplate OnStop()
        {
            Intraday();
            ParamsSetterUtility.SetTriggerOnStop(paramsDictionary);
            return this;
        }

        public BaseTemplate WithStopTypeOf(StopPriceCalculationStrategy strategy)
        {
            OnStop();
            ParamsSetterUtility.SetStopCalculationStrategy(paramsDictionary, strategy);
            return this;
        }

        public BaseTemplate WithStopPriceReferenceOf(StopCalculationReferencePriceStrategy strategy)
        {
            ParamsSetterUtility.SetStopCalculationReferenceStrategy(paramsDictionary, strategy);
            return this;
        }

        public BaseTemplate OnIwmStop()
        {
            OnStop();
            ParamsSetterUtility.SetTriggerStrategy(paramsDictionary, OrderTriggerStrategy.IwmStopPriceBasedTrigger);
            return this;
        }

        public BaseTemplate WithIwmStopTypeOf(StopPriceCalculationStrategy strategy)
        {
            OnStop();
            ParamsSetterUtility.SetIwmStopCalculationStrategy(paramsDictionary, strategy);
            return this;
        }

        public BaseTemplate WithIwmStopPriceReferenceOf(StopCalculationReferencePriceStrategy strategy)
        {
            ParamsSetterUtility.SetIwmStopCalculationReferenceStrategy(paramsDictionary, strategy);
            return this;
        }

        public BaseTemplate WithIwmOpeningGapPriceReferenceOf(StopCalculationReferencePriceStrategy strategy)
        {
            ParamsSetterUtility.SetIwOpeningGapReferenceStrategy(paramsDictionary, strategy);
            return this;
        }


        public BaseTemplate WithStopTypeOfProtectiveStop()
        {
            OnStop();
            WithStopTypeOf(StopPriceCalculationStrategy.ProtectiveStopBasedOnAtr);
            WithStopPriceRefefenceOfCurrentDayHiLo();
            return this;
        }

        public BaseTemplate WithStopTypeOfRetracementStop()
        {
            OnStop();
            WithStopTypeOf(StopPriceCalculationStrategy.RetracementEntryBasedOnAtr);
            WithStopPriceRefefenceOfCurrentDayHiLo();
            return this;
        }

        public BaseTemplate WithStopTypeOfAbIfStop()
        {
            OnStop();
            WithStopTypeOf(StopPriceCalculationStrategy.AbIfStopBasedOnAtr);
            WithStopPriceRefefenceOfPreviousDayHiLo();
            return this;
        }

        public BaseTemplate WithStopTypeOfFixedAmount()
        {
            OnStop();
            WithStopTypeOf(StopPriceCalculationStrategy.FixedAmount);
            return this;
        }


        public BaseTemplate WithStopPriceRefefenceOfPreviousDayHiLo()
        {
            WithStopPriceReferenceOf(StopCalculationReferencePriceStrategy.PreviousDayPrice);
            return this;
        }

        public BaseTemplate WithStopPriceRefefenceOfPreviousDayClose()
        {
            WithStopPriceReferenceOf(StopCalculationReferencePriceStrategy.PreviousDayClosePrice);
            return this;
        }

        public BaseTemplate WithStopPriceRefefenceOfCurrentDayHiLo()
        {
            WithStopPriceReferenceOf(StopCalculationReferencePriceStrategy.CurrentDayHiLoPrice);
            return this;
        }

        public BaseTemplate WithStopPriceRefefenceOfCurrentDayOpen()
        {
            WithStopPriceReferenceOf(StopCalculationReferencePriceStrategy.CurrentDayOpenPrice);
            return this;
        }

    }

    internal class TestTemplate
    {
        //private ProjectToRun prj = new ProjectToRun(
        //    CloseOrderTemplate.AtCloseOrder(CloseOrderProjectNames.Close_04)
        //        .OnStop()
        //        .WithStopTypeOfAbIfStop()
        //        .WithStopPriceRefefenceOfPreviousDayHiLo()
        //    )
        //    .AtHour(10)
        //    .AtMinute(22)
        //    .WithAmount(22222)
        //    .WithSolutionParameters(null)
        //    .WithStockSymbols(@"");
    }
}