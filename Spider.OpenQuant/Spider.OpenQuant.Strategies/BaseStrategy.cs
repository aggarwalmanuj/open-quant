using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using OpenQuant.API;
using OpenQuant.API.Indicators;
using Spider.OpenQuant.Strategies.Util;

namespace Spider.OpenQuant.Strategies
{
    public abstract partial class BaseStrategy : Strategy
    {
        #region Trigger Management
        [Parameter("Time", "Trigger Management")]
        public DateTime TiggerDate = DateTime.Today;

        [Parameter("Trigger Hour", "Trigger Management")]
        public int TriggerHour = 6;

        [Parameter("Trigger Minute", "Trigger Management")]
        public int TriggerMinute = 30;

        [Parameter("Trigger Second", "Trigger Management")]
        public int TriggerSecond = 0;

        [Parameter("Retry Trigger Interval Minute", "Trigger Management")]
        public int RetryTriggerIntervalMinute = 15;

        [Parameter("Enable Retry Trigger", "Trigger Management")]
        public bool EnableRetryTrigger = false;


        [Parameter("MaximumRetries", "Trigger Management")]
        public int MaximumRetries = 5;


        #endregion

        #region Money Management


        [Parameter("Position Size Percentage", "Money Management")]
        public double PositionSizePercentage = 100;

        #endregion


        #region Other Management

        [Parameter("Persist Historical Data", "Other Options")]
        public bool PersistHistoricalData = true; 
        #endregion

        #region Order Management

        [Parameter("Auto Submit", "Order Management")]
        public bool AutoSubmit = true;

        [Parameter("Use Market Order", "Order Management")]
        public bool UseMarketOrder = false;
        #endregion

        #region Log Management


        [Parameter("Log Debug Enabled", "Log Management")]
        public bool IsLogDebugEnabled = true;

        [Parameter("Log Warn Enabled", "Log Management")]
        public bool IsLogWarnEnabled = true;

        [Parameter("Log Info Enabled", "Log Management")]
        public bool IsLogInfoEnabled = true;


        #endregion

        protected Dictionary<string, BarSeries> minutelyBarSeriesDictionary =
            new Dictionary<string, BarSeries>(StringComparer.InvariantCultureIgnoreCase);
        protected Dictionary<string, BarSeries> dailyBarSeriesDictionary =
            new Dictionary<string, BarSeries>(StringComparer.InvariantCultureIgnoreCase);


        //protected BarSeries minutelyBarSeries = null;
        //protected BarSeries dailyBarSeries = null;
        protected DateTime? triggerTime = null;
        protected Order strategyOrder = null;
        private static readonly object LockObject = new object();
        private StopPriceManager stopPriceManager = null;
        protected Dictionary<string, ATR> atrDictionary = new Dictionary<string, ATR>();
        protected LoggingConfig logConf = null;
        private PositionSizeManager posSizeMgr;
        protected int retryCount = 0;

        protected void InitializeStrategy()
        {
            if (!triggerTime.HasValue)
                triggerTime = TiggerDate.Date.AddHours(TriggerHour).AddMinutes(TriggerMinute).AddSeconds(TriggerSecond);
            Console.WriteLine();
            Console.WriteLine("!!!! REMEMBER TO CHANGE STOPS AND QUANTITIES IN CODE !!!!");
            Console.WriteLine();

            ResetData();
        }

        private void ResetData()
        {
            DateTime start = DateTime.Now;
            try { DataManager.DeleteDailySeries(Instrument); } catch { }
            try { DataManager.DeleteQuoteSeries(Instrument); } catch { }
            try { DataManager.DeleteTradeSeries(Instrument); } catch { }

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

            DateTime end = DateTime.Now;
            LoggingUtility.WriteInfo(LoggingConfig, string.Format("Reset instrument data in {0}ms", end.Subtract(start).TotalMilliseconds));
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrument"></param>
        protected void PreLoadBarData(Instrument instrument)
        {
            PreLoadDailyDataImpl(instrument, BarDuration.Minutely);

            PreLoadDailyDataImpl(instrument, BarDuration.Daily);
        }

        private void PreLoadDailyDataImpl(Instrument instrument, BarDuration  duration)
        {
            BarSeries series = null;
            string id = instrument.ToIdentifier();
            Dictionary<string, BarSeries> dictionaryToUse = null;

            if (duration == BarDuration.Daily)
                dictionaryToUse = dailyBarSeriesDictionary;
            else if (duration == BarDuration.Minutely)
                dictionaryToUse = minutelyBarSeriesDictionary;
            else
                throw new ArgumentOutOfRangeException("duration", duration, "Incorrect value for duration");

            dictionaryToUse.TryGetValue(id, out series);
            if (null != series && series.Count >= 1)
                return;

            DateTime start = DateTime.Now;
            if (duration == BarDuration.Daily)
                series = GetHistoricalBars("IB", Instrument, DateTime.Now.AddDays(-60), DateTime.Now, PeriodConstants.PERIOD_DAILY);
            else if (duration == BarDuration.Minutely)
                series = GetHistoricalBars("IB", instrument, DateTime.Now.AddDays(-5), DateTime.Now, PeriodConstants.PERIOD_MINUTE);
            DateTime end = DateTime.Now;


            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Took {0}ms to retrieve data from IB for {1} data", end.Subtract(start).TotalMilliseconds, duration));

            dictionaryToUse[id] = series;

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


        protected void AddStopPrice(string symbol, double price)
        {
            if (string.Compare(Instrument.Symbol, symbol, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                StopPriceManager.AddStopPrice(symbol, price);

                LoggingUtility.WriteDebug(LoggingConfig, string.Format("Added a stop price of {0:c}", price));
            }
        }

        protected void AddPositionSize(string symbol, PositionSide positionSide, double positionSize)
        {
            if (string.Compare(Instrument.Symbol, symbol, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                double qty = (positionSide == PositionSide.Long) ? Math.Abs(positionSize) : Math.Abs(positionSize)*-1;
                PositionSizeManager.AddPositionSize(symbol, qty);

                LoggingUtility.WriteDebug(LoggingConfig, string.Format("Added a position size of {0}", positionSize));
            }
        }

        protected StopPriceManager StopPriceManager
        {
            get
            {
                if (null == stopPriceManager)
                {
                    lock (LockObject)
                    {
                        if (null == stopPriceManager)
                            stopPriceManager = new StopPriceManager(LoggingConfig);
                    }
                }
                return stopPriceManager;
            }
        }


        protected PositionSizeManager PositionSizeManager
        {
            get
            {
                if (null == posSizeMgr)
                {
                    lock (LockObject)
                    {
                        if (null == posSizeMgr)
                            posSizeMgr = new PositionSizeManager(LoggingConfig);
                    }
                }
                return posSizeMgr;
            }
        }

        protected LoggingConfig LoggingConfig
        {
            get
            {
                if (null == logConf)
                {
                    lock (LockObject)
                    {
                        if (null == logConf)
                        {
                            logConf = new LoggingConfig()
                                          {
                                              IsDebugEnabled = IsLogDebugEnabled,
                                              IsWarnEnabled = IsLogWarnEnabled,
                                              IsInfoEnabled = IsLogInfoEnabled,
                                              Instrument = Instrument
                                          };
                        }
                    }
                }
                return logConf;
            }
        }

     

        protected ATR GetAtr(Instrument instrument, int period)
        {
            BarSeries dailyBarSeries = null;

            string instId = instrument.ToIdentifier();
            dailyBarSeriesDictionary.TryGetValue(instId, out dailyBarSeries);

            if (dailyBarSeries == null || dailyBarSeries.Count <= 0)
                throw new ApplicationException("Daily bar series has not been initialized");


            string atrId = string.Format("{0}:{1}", instId, period);

            if (!atrDictionary.ContainsKey(atrId))
            {
                lock (LockObject)
                {
                    if (!atrDictionary.ContainsKey(atrId))
                    {
                        ATR atr = new ATR(dailyBarSeries, period);
                        atrDictionary.Add(atrId, atr);
                    }
                }
            }

            return atrDictionary[atrId];
        }

       

        protected virtual bool IsItTimeToTrigger(Bar bar, bool logBar)
        {
            bool returnValue = false;

            try
            {
                returnValue = (triggerTime.HasValue) && (bar.BeginTime >= triggerTime) && (strategyOrder == null);
            }
            finally
            {
                if (returnValue && logBar)
                    LoggingUtility.LogOkToTriggerOrder(LoggingConfig, bar);
            }

          
            return returnValue;
        }

        protected virtual bool IsItTimeToRetryOrder(Bar bar)
        {
            bool returnValue = false;

            if (EnableRetryTrigger && retryCount < MaximumRetries)
            {
                if (null != strategyOrder && !(strategyOrder.IsFilled || strategyOrder.IsPartiallyFilled))
                {
                    if (bar.EndTime.Subtract(triggerTime.Value).TotalSeconds >= (RetryTriggerIntervalMinute*60))
                    {
                        returnValue = true;
                    }
                }
            }

            return returnValue;
        }


        protected virtual bool RetryOrder(Bar bar, HandlerStrategyStartHandler strategyStartHandler)
        {
            bool returnValue = false;

            if (IsItTimeToRetryOrder(bar))
            {
                try
                {
                    /*
                    while (!strategyOrder.IsCancelled)
                    {
                        strategyOrder.Cancel();
                        Thread.Sleep(1);
                    }
                     */

                    strategyOrder.Cancel();
                    strategyOrder = null;
                    triggerTime = triggerTime.Value.AddMinutes(RetryTriggerIntervalMinute);
                    //strategyStartHandler();
                    retryCount++;

                    returnValue = true;
                }
                catch (Exception ex)
                {
                    LoggingUtility.WriteError(LoggingConfig,
                                              string.Format("Error happened while trying to retry order: {0}", ex));
                }
                finally
                {
                }
            }

            return returnValue;
        }


        protected string GetAutoPlacedOrderName(OrderSide orderSide, string info, string instrument)
        {
            if (string.IsNullOrEmpty(info))
                return string.Format("{0} order for {1}", orderSide, instrument);
            else
                return string.Format("{0} ({1}) order {2}", orderSide, info, instrument);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        /// <returns></returns>
        protected Bar GetPreviousBar(Instrument instrument, Bar bar, int period)
        {
            Bar retVal = null;
            BarSeries barsToUse = null;
            string instId = instrument.ToIdentifier();
            Dictionary<string, BarSeries> dictionaryToUse = null;

            bool isSessionOpenBar = bar.IsSessionOpenBar(Instrument.Type);
            bool isDailyPeriod = period == PeriodConstants.PERIOD_DAILY;

            if (isDailyPeriod) // || isSessionOpenBar
                dictionaryToUse = dailyBarSeriesDictionary;
            else
                dictionaryToUse = minutelyBarSeriesDictionary;

            barsToUse = dictionaryToUse[instId];

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
                throw new ApplicationException(string.Format("Count not retreive a period {0} bar to {1}", period, bar));

            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Previous closing bar was {0}", retVal));

            return retVal;
        }

        protected Bar GetPreviousDayBar(Instrument instrument)
        {
            Bar retVal = null;
            string instId = instrument.ToIdentifier();
            BarSeries barsToUse = dailyBarSeriesDictionary[instId];

            if (barsToUse.Count > 0)
            {
                int idx = 0;
                bool found = false;

                while (!found && idx <= barsToUse.Count - 1)
                {
                    Bar prevBar = barsToUse.Ago(idx);
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
                throw new ApplicationException(string.Format("Count not retreive a daily bar prior to {0}", DateTime.Today));

            LoggingUtility.WriteDebug(LoggingConfig, string.Format("Previous closing bar was {0}", retVal));

            return retVal;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="orderSide"></param>
        /// <param name="qty"></param>
        /// <param name="name"></param>
        /// <param name="limitPrice"></param>
        /// <returns></returns>
        protected Order CreateOrder(OrderSide orderSide, double qty, string name, double? limitPrice)
        {
            Order returnValue = null;

            if (UseMarketOrder)
                returnValue = MarketOrder(orderSide, qty, name + "--MARKET--");
            else
                returnValue = LimitOrder(orderSide, qty, limitPrice.Value, name);

            return returnValue;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="instrument"></param>
        /// <param name="atrPeriod"></param>
        /// <param name="triggerTime"></param>
        /// <returns></returns>
        protected double GetAtrValue(Instrument instrument, int atrPeriod, DateTime triggerTime)
        {
            ATR atr = GetAtr(instrument, atrPeriod);
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
                throw new ApplicationException(string.Format("Count not retrieve an ATR for {0} before bar {1}", instrument.Symbol, triggerTime));

            LoggingUtility.WriteInfo(LoggingConfig, string.Format("Found ATR value of {0:c} as of {1}", returnValue, atr.GetDateTime(idx)));

            return returnValue;
        }
    }
}



