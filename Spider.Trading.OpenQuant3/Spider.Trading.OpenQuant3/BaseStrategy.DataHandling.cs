using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Calculations.StopPriceCalc;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Factories;
using Spider.Trading.OpenQuant3.Util;
using Spider.Trading.OpenQuant3.Validators;

namespace Spider.Trading.OpenQuant3
{
    public abstract partial class BaseStrategy
    {
        

        private void SetupData()
        {
            ResetData();

            PreLoadBarData();

            Bar previousDayBar = GetPreviousDayBar();

            EffectivePreviousClosePrice = previousDayBar.Close;
            EffectivePreviousHiPrice = previousDayBar.High;
            EffectivePreviousLoPrice = previousDayBar.Low;

            Bar currentDayBar = GetCurrentDayBar();

            if (null != currentDayBar)
            {
                EffectiveCurrentDayHiPrice = currentDayBar.High;
                EffectiveCurrentDayLoPrice = currentDayBar.Low;
                EffectiveOpenPrice = currentDayBar.Open;
            }


            EffectiveAtrPrice = GetAtrValue(AtrPeriod, EffectiveValidityTriggerTime, EffectivePreviousClosePrice);

            LoggingUtility.WriteInfo(LoggingConfig,string.Format("Found Previous Close Price Of {0:c} and ATR of {1:c}.",EffectivePreviousClosePrice,EffectiveAtrPrice));
        }




        #region Data Management


        private void ResetData()
        {
            DateTime start = DateTime.Now;

            try { DataManager.DeleteBarSeries(Instrument, BarType.Time, PeriodConstants.PERIOD_MINUTE); }
            catch { }
            try { DataManager.DeleteDailySeries(Instrument); }
            catch { }
            try { DataManager.DeleteQuoteSeries(Instrument); }
            catch { }
            try { DataManager.DeleteTradeSeries(Instrument); }
            catch { }
            try { DataManager.Flush(); }
            catch { }

            /*
            BarSeriesInfo[] barInfos = DataManager.GetBarSeriesInfoList(Instrument);
            if (null != barInfos)
            {
                foreach (var barSeriesInfo in barInfos)
                {
                    try
                    {
                        DataManager.DeleteBarSeries(Instrument, barSeriesInfo.BarType, barSeriesInfo.BarSize);
                    }
                    catch
                    {
                    }
                }
            }
            */

            DateTime end = DateTime.Now;
            LoggingUtility.WriteVerbose(LoggingConfig, string.Format("Reset instrument data in {0}ms", end.Subtract(start).TotalMilliseconds));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrument"></param>
        protected void PreLoadBarData()
        {
            PreLoadDataImpl(BarDuration.Minutely);

            PreLoadDataImpl(BarDuration.Daily);
        }

        private void PreLoadDataImpl(BarDuration duration)
        {
            BarSeries series = null;
           

            DateTime start = DateTime.Now;
            if (duration == BarDuration.Daily)
            {
                DailyBarSeries = GetHistoricalBars("IB", Instrument, DateTime.Now.AddDays(-60), DateTime.Now,
                                                   PeriodConstants.PERIOD_DAILY);
                series = DailyBarSeries;
            }
            else if (duration == BarDuration.Minutely)
            {
                int dayToGet = Math.Abs(DaysToGoBackForMinutelyData)*-1;
                DateTime startDateForMinutelyData = EffectiveValidityTriggerTime.Date.AddDays(dayToGet);
                if (DaysToGoBackForMinutelyData != 0)
                {
                    startDateForMinutelyData = EffectiveValidityTriggerTime.Date.AddDays(dayToGet);
                }
                else
                {
                    dayToGet = -2;
                    startDateForMinutelyData = EffectiveValidityTriggerTime.Date.AddDays(dayToGet);
                    bool isHoliday = startDateForMinutelyData.Date.DayOfWeek == DayOfWeek.Saturday ||
                                     startDateForMinutelyData.Date.DayOfWeek == DayOfWeek.Sunday;

                    while (isHoliday)
                    {
                        startDateForMinutelyData = startDateForMinutelyData.AddDays(-1);
                        isHoliday = startDateForMinutelyData.Date.DayOfWeek == DayOfWeek.Saturday ||
                                    startDateForMinutelyData.Date.DayOfWeek == DayOfWeek.Sunday;
                    }
                }

                LoggingUtility.WriteDebug(LoggingConfig, string.Format("Trying to retrieve minutely data starting from {0} from IB", startDateForMinutelyData));


                MinutelyBarSeries = GetHistoricalBars("IB", Instrument, startDateForMinutelyData, DateTime.Now,
                                                      PeriodConstants.PERIOD_MINUTE);
                series = MinutelyBarSeries;
            }
            DateTime end = DateTime.Now;

            int retrievedBarCount = 0;
            if (null != series)
                retrievedBarCount = series.Count;

            LoggingUtility.WriteDebug(LoggingConfig,
                                      string.Format(
                                          "Took {0}ms to retrieve data from IB for {1} data. Total bars retrieved: {2}",
                                          end.Subtract(start).TotalMilliseconds, duration, retrievedBarCount));


            
            start = DateTime.Now;
            foreach (Bar currentBar in series)
            {
                Bars.Add(currentBar);
                if (PersistHistoricalData)
                    DataManager.Add(Instrument, currentBar);
            }
            end = DateTime.Now;


            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Took {0}ms to load data into memory for {1} data", end.Subtract(start).TotalMilliseconds, duration));
        }


        /*
        protected void ReloadDailyData()
        {
            DateTime start = DateTime.Now;
            DailyBarSeries = GetHistoricalBars("IB", Instrument, DateTime.Now.AddDays(-60), DateTime.Now, PeriodConstants.PERIOD_DAILY);
            DateTime end = DateTime.Now;


            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Took {0}ms to retrieve data from IB for daily data", end.Subtract(start).TotalMilliseconds));

            start = DateTime.Now;
            foreach (Bar currentBar in DailyBarSeries)
            {
                Bars.Add(currentBar);
                if (PersistHistoricalData)
                    DataManager.Add(Instrument, currentBar);
            }
            end = DateTime.Now;

            LoggingUtility.WriteVerbose(LoggingConfig, string.Format("Took {0}ms to load data into memory for daily data", end.Subtract(start).TotalMilliseconds));
        }*/




        protected Bar GetPreviousBar(Bar bar, int period)
        {
            Bar retVal = null;
            BarSeries barsToUse = null;

            bool isSessionOpenBar = bar.IsSessionOpenBar(Instrument.Type);
            bool isDailyPeriod = period == PeriodConstants.PERIOD_DAILY;

            if (isDailyPeriod)
                return GetPreviousDayBar();

            barsToUse = MinutelyBarSeries;

            if (barsToUse.Count > 0)
            {
                int idx = 0;
                bool found = false;

                while (!found && idx <= barsToUse.Count - 1)
                {
                    Bar prevBar = barsToUse.Ago(idx);
                    if ((prevBar.EndTime <= bar.BeginTime) && prevBar.IsWithinRegularTradingHours(Instrument.Type))
                    {
                        if (isSessionOpenBar || isDailyPeriod)
                        {
                            found = DateTime.Today.Subtract(prevBar.BeginTime.Date).TotalDays >= 1;
                            if (!found && DateTime.Now.IsPastRegularTradingHours(Instrument.Type))
                                found = DateTime.Today.Subtract(prevBar.BeginTime.Date).TotalDays >= 0;
                        }
                        else
                        {
                            found = true;
                        }
                    }

                    if (found)
                        retVal = prevBar;
                    else
                        idx++;
                }
            }


            if (retVal == null)
                throw new ApplicationException(string.Format("Count not retreive a period {0} bar to {1}. If it is due to exchange holidays - then set the 'DaysToGoBackForMinutelyData' parameter to fetch more data.", period, bar));

            LoggingUtility.WriteInfo(LoggingConfig, string.Format("Previous closing bar was {0}", retVal));

            return retVal;
        }


        protected Bar GetCurrentDayBar()
        {
            Bar retVal = null;

            if (DailyBarSeries.Count > 0)
            {
                int idx = 0;
                bool found = false;

                while (!found && idx <= DailyBarSeries.Count - 1)
                {
                    Bar prevBar = DailyBarSeries.Ago(idx);
                    if (prevBar.EndTime.Date <= DateTime.Today)
                    {
                        if (prevBar.IsWithinRegularTradingHours(Instrument.Type))
                        {
                            found = DateTime.Today.Subtract(prevBar.BeginTime.Date).TotalDays >= 0;
                            if (!found && DateTime.Now.IsPastRegularTradingHours(Instrument.Type))
                                found = DateTime.Today.Subtract(prevBar.BeginTime.Date).TotalDays >= 0;
                        }
                    }

                    if (found)
                        retVal = prevBar;
                    else
                        idx++;
                }
            }


            if (retVal == null)
                throw new ApplicationException(string.Format("Count not retreive the current daily bar {0}. If it is due to exchange holidays - then set the 'DaysToGoBackForMinutelyData' parameter to fetch more data.", DateTime.Today));

            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Current daily bar was {0}", retVal));

            return retVal;
        }

        protected Bar GetPreviousDayBar()
        {
            Bar retVal = null;

            if (DailyBarSeries.Count > 0)
            {
                int idx = 0;
                bool found = false;

                while (!found && idx <= DailyBarSeries.Count - 1)
                {
                    Bar prevBar = DailyBarSeries.Ago(idx);
                    if (prevBar.EndTime.Date <= DateTime.Today)
                    {
                        if (prevBar.IsWithinRegularTradingHours(Instrument.Type))
                        {
                            found = DateTime.Today.Subtract(prevBar.BeginTime.Date).TotalDays >= 1;
                            if (!found && DateTime.Now.IsPastRegularTradingHours(Instrument.Type))
                                found = DateTime.Today.Subtract(prevBar.BeginTime.Date).TotalDays >= 0;
                        }
                    }

                    if (found)
                        retVal = prevBar;
                    else
                        idx++;
                }
            }


            if (retVal == null)
                throw new ApplicationException(string.Format("Count not retreive a daily bar prior to {0}. If it is due to exchange holidays - then set the 'DaysToGoBackForMinutelyData' parameter to fetch more data.", DateTime.Today));

            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Previous closing bar was {0}", retVal));

            return retVal;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="atrPeriod"></param>
        /// <param name="triggerTime"></param>
        /// <returns></returns>
        private double GetAtrValue(int atrPeriod, DateTime triggerTime, double price)
        {
            ATR atr = new ATR(DailyBarSeries, atrPeriod);
            double returnValue = 0;
            int idx = atr.Count - 1;
            bool found = false;

            while (!found && idx >= 0)
            {
                if (atr.GetDateTime(idx) < triggerTime)
                {
                    found = true;
                    returnValue = atr[idx];
                    break;
                }
                idx--;
            }

            if (!found || returnValue <= 0)
                throw new ApplicationException(string.Format("Count not retrieve an ATR for {0} before bar {1}. If it is due to exchange holidays - then set the 'DaysToGoBackForMinutelyData' parameter to fetch more data.", Instrument.Symbol, triggerTime));

            LoggingUtility.WriteInfo(LoggingConfig, string.Format("Found ATR value of {0:c} ({2:p}) as of {1}", returnValue, atr.GetDateTime(idx), returnValue / price ));

            return returnValue;
        }


        private bool IsBarValid(Bar bar)
        {
            return (bar.Open > 0 || bar.Close > 0) && bar.High > 0 && bar.Low > 0;
        }


        #endregion

        

        private void SetAndValidateStopPrice()
        {

            if (IsCurrentInstrumentIwmAndIgnorable())
            {
                StopCalculationStrategy = IwmStopCalculationStrategy;
                 
                CalculatedStopAtrCoefficient = IwmCalculatedStopAtrCoefficient;

                OpeningGapAtrCoefficient = IwmOpeningGapAtrCoefficient;

                StopCalculationReferencePriceStrategy = IwmStopCalculationReferencePriceStrategy;

                OpeningGapReferencePriceStrategy = IwmOpeningGapReferencePriceStrategy;
            }

            if (IsStopPriceApplicableToThisInstrument())
            {
                StopPriceCalculator = StopPriceCalculatorFactory.GetCalculator(this);
                StopPriceCalculator.Calculate(null);
            }

        }


        private void RecalculateStopPriceBasedOnNewHiLoForTheDay(Bar bar)
        {
            if (StopCalculationReferencePriceStrategy == StopCalculationReferencePriceStrategy.PreviousDayPrice
                || StopCalculationReferencePriceStrategy == StopCalculationReferencePriceStrategy.PreviousDayClosePrice
                || StopCalculationReferencePriceStrategy == StopCalculationReferencePriceStrategy.CurrentDayOpenPrice)
                return;

            if (bar.BeginTime < EffectiveStartOfSessionTime)
                return;

            if (IsValidityTimeTriggered)
            {
                // within the session - some stops do not need to be updated
                if (null != StopPriceCalculator)
                {
                    try
                    {
                        if (!(StopPriceCalculator as BaseStopPriceCalculator).NeedToUpdateCurrentDayHiLoPrice())
                        {
                            return;
                        }
                    }
                    catch (Exception exWhileUpdatingHiLoPrice)
                    {
                        LoggingUtility.WriteError(LoggingConfig, exWhileUpdatingHiLoPrice.ToString());
                    }
                }
            }

            bool recalculateStopRequired = false;

            if (bar.High > 0 && bar.High > EffectiveCurrentDayHiPrice)
            {
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Changing current day hi price from {0:c} to {1:c}",
                                                       EffectiveCurrentDayHiPrice, bar.High));

                EffectiveCurrentDayHiPrice = bar.High;
                recalculateStopRequired = true;
            }

            if (bar.Low > 0 && bar.Low < EffectiveCurrentDayLoPrice)
            {
                LoggingUtility.WriteInfo(LoggingConfig,
                                         string.Format("Changing current day lo price from {0:c} to {1:c}",
                                                       EffectiveCurrentDayLoPrice, bar.Low));

                EffectiveCurrentDayLoPrice = bar.Low;
                recalculateStopRequired = true;
            }


            if (recalculateStopRequired)
            {
                if (IsStopPriceApplicableToThisInstrument() && StopPriceCalculator != null)
                {
                    StopPriceCalculator.Calculate(null);
                }
            }
        }


        protected void UpdateRetrialStopPrice(Bar bar)
        {
            OrderSide orderSide = EffectiveOrderSide;
            double atrValue = EffectiveAtrPrice;
            double atrCoeff = AdverseMovementInPriceAtrThreshold;

            switch (orderSide)
            {
                case OrderSide.Buy:
                    // Protective is to make sure price does not higher than yesterday's high
                    EffectiveAdverseMovementInPriceAtrThresholdForRetrialValue = EffectiveLastOrderPrice + (atrValue * atrCoeff);
                    break;


                case OrderSide.Sell:
                    // Protective is to make sure price does not go lower than yesterday's low
                    EffectiveAdverseMovementInPriceAtrThresholdForRetrialValue = EffectiveLastOrderPrice - (atrValue * atrCoeff);
                    break;


                default:
                    throw new NotImplementedException();

            }
        }

        
    }
}
