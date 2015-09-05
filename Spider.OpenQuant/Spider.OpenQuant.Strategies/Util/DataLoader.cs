//using System;
//using System.IO;
//using System.Collections;

//using OpenQuant.API;
//using Spider.OpenQuant.Strategies.Util;


//namespace Spider.OpenQuant.Strategies
//{

//    public abstract partial class BaseStrategy : Strategy
//    {
//    //    [Parameter("Time of Day Start (localtime)")]
//    //    TimeSpan StartTime = new TimeSpan(08, 30, 0); // 8:30 AM CST
//    //    [Parameter("Time of Day End (localtime)")]
//    //    TimeSpan EndTime = new TimeSpan(15, 15, 0); // 3:15 PM CST
//    //    [Parameter("Debug Messages")]
//    //    bool DebugMessages = false;
//    //    [Parameter("StartDate")]
//    //    DateTime StartDate = new DateTime(2008, 1, 2);
//    //    [Parameter("StopDate")]
//    //    DateTime StopDate = new DateTime(2008, 5, 1);

//    //    [Parameter("BID/ASK")]
//        //bool Bid_Ask = true;	// set to true to load quotes and build bars from quotes

//        //private ProviderError ibProviderError = null;
//        //private bool ibProviderDisconnect = false;
//        //private int allInstrumentTotalCount = 0;
//        //private DateTime LastTime = DateTime.Now;
//        //private DateTime FirstTime = DateTime.Now;
//        //private int RequestCount = 0;

//        public void LoadRequestedData()
//        {
//            int InstrumentAddCount = 0;

//            if (!DataRequests.HasBarRequests)
//            {
//                throw (new ApplicationException("No BarRequests in Strategy"));
//            }
//            else
//            {
//                foreach (BarRequest currentBarRequest in DataRequests.BarRequests)
//                {
//                    ProcessHistoricalBarRequest(currentBarRequest);
//                }
//            }
//            Console.WriteLine("OnStrategyStart End for " + Instrument.ToString());
//            Console.WriteLine("Added " + InstrumentAddCount + " Bars for " + Instrument.ToString());
//        }


//        private int ProcessHistoricalBarRequest(BarRequest currentBarRequest)
//        {
//            int numberOfRequiredBars;

//            DateTime tempDateCounterStart;
//            DateTime tempDateCounterEnd;

//            int historicalBarsFoundInLocalDb;
//            BarSeries historicalBarsFoundOnlineInProvider;
//            Bar currentBarToBeAdded;
//            int AddCount;

//            if (LoggingConfig.IsDebugEnabled)
//                LoggingUtility.WriteDebug(LoggingConfig,
//                                          string.Format(
//                                              "Processing historical bar request. BarSize: {0}, BarType: {1}",
//                                              currentBarRequest.BarSize, currentBarRequest.BarType));

//            switch (currentBarRequest.BarSize)
//            {
//                case PeriodConstants.PERIOD_DAILY:
//                    numberOfRequiredBars = MinNumberOfBars.DailyBars;
//                    tempDateCounterStart = DateTime.Now.AddDays(-60);
//                    break;
//                case PeriodConstants.PERIOD_MINUTE:
//                    numberOfRequiredBars = MinNumberOfBars.MinuteBars;
//                    tempDateCounterStart = DateTime.Now.AddDays(-5);
//                    break;
//                default:
//                    throw new NotSupportedException(string.Format(
//                        "Cannot process bar request. Unsupported bar size: {0}", currentBarRequest.BarSize));
//            }
//            tempDateCounterStart = StartDate;

//            while (tempDateCounterStart < StopDate && tempDateCounterStart < DateTime.Now)
//            {
//                if (ibProviderDisconnect)
//                    throw new System.Exception("IB Provider Disconnect.  Please connect and retry.");

//                if (tempDateCounterStart.DayOfWeek == DayOfWeek.Saturday)
//                    tempDateCounterStart = tempDateCounterStart.AddDays(2);

//                if (tempDateCounterStart.DayOfWeek == DayOfWeek.Sunday)
//                    tempDateCounterStart = tempDateCounterStart.AddDays(1);

//                // if((D.TimeOfDay>=StartTime)&&(D.TimeOfDay<EndTime)&&(D.Date==D.AddSeconds(NumberOfBars*BR.BarSize).Date)) {
//                if ((tempDateCounterStart.TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds) && (tempDateCounterStart.TimeOfDay.TotalSeconds < PstSessionTimeConstants.StockExchangeEndTimeSeconds))
//                {
//                    Console.WriteLine(DateTime.Now + " Processing " + Instrument.ToString() + " " + tempDateCounterStart);

//                    tempDateCounterEnd = tempDateCounterStart.AddSeconds(numberOfRequiredBars * currentBarRequest.BarSize);
//                    if (tempDateCounterEnd > (tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds)))
//                        tempDateCounterEnd = tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds);

//                    if (LoggingConfig.IsDebugEnabled) Console.WriteLine("Looking from\t" + tempDateCounterStart);
//                    if (LoggingConfig.IsDebugEnabled) Console.WriteLine("Looking to  \t" + tempDateCounterEnd);

//                    historicalBarsFoundInLocalDb = DataManager.GetHistoricalBars(Instrument, tempDateCounterStart, tempDateCounterEnd.AddSeconds(-1), currentBarRequest.BarType, currentBarRequest.BarSize).Count;
//                    if (LoggingConfig.IsDebugEnabled) Console.WriteLine("DataManager DMFound \t" + historicalBarsFoundInLocalDb);

//                    if (historicalBarsFoundInLocalDb < numberOfRequiredBars)
//                    {
//                        bool done = false;
//                        do
//                        {
//                            TimeSpan t = DateTime.Now.Subtract(LastTime);

//                            // Delay by at least 2 seconds so not to overload HMDS server
//                            if (t.TotalMilliseconds < 2000)
//                                System.Threading.Thread.Sleep(2000 - (int)t.TotalMilliseconds);

//                            if ((RequestCount++) == (Bid_Ask ? 30 : 60))
//                            {
//                                t = DateTime.Now.Subtract(FirstTime);
//                                // if 60 request and been less than 10 min, go to sleep until past 10min
//                                // so not to get pacing violation
//                                if (t.TotalSeconds < 10 * 60)
//                                {
//                                    Console.WriteLine(DateTime.Now + "\tGoing to Sleep.");
//                                    TimeSpan SleepTime = new TimeSpan(0, 10, 30) - t;
//                                    Console.WriteLine("Sleeping for " + SleepTime);
//                                    System.Threading.Thread.Sleep(SleepTime);
//                                    Console.WriteLine(DateTime.Now + "\tWaking up.");
//                                }
//                                RequestCount = 0;
//                                FirstTime = DateTime.Now;
//                            }
//                            LastTime = DateTime.Now;
//                            ibProviderError = null;

//                            tempDateCounterEnd = tempDateCounterStart.AddSeconds(numberOfRequiredBars * currentBarRequest.BarSize);
//                            if (tempDateCounterEnd > (tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds)))
//                                tempDateCounterEnd = tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds);

//                            historicalBarsFoundOnlineInProvider = DataManager.GetHistoricalBars("IB", Instrument, tempDateCounterStart, tempDateCounterEnd, (int)currentBarRequest.BarSize);
//                            if (LoggingConfig.IsDebugEnabled) Console.WriteLine(DateTime.Now + " IB Returned Count " + historicalBarsFoundOnlineInProvider.Count);

//                            if (historicalBarsFoundOnlineInProvider.Count <= 0)
//                            {
//                                for (int i = 0; (i < 10) && (ibProviderError == null); i++)
//                                {
//                                    System.Threading.Thread.Sleep(100);
//                                }

//                                if ((ibProviderError != null) && (ibProviderError.Code == 162) && (ibProviderError.Message.Contains("pacing")))
//                                {
//                                    Console.WriteLine(DateTime.Now + "\tPacing Violation.  Going to Sleep.");
//                                    // sleep for over 10 minutes to insure HMDS server is reset
//                                    TimeSpan SleepTime = new TimeSpan(0, 10, 30);
//                                    Console.WriteLine("Sleeping for " + SleepTime);
//                                    System.Threading.Thread.Sleep(SleepTime);
//                                    Console.WriteLine(DateTime.Now + "\tWaking up.  Try Again.");
//                                    RequestCount = 0;
//                                    FirstTime = DateTime.Now;
//                                }
//                                else if ((ibProviderError != null) && (ibProviderError.Code == 162) && (ibProviderError.Message.Contains("no data")))
//                                {
//                                    done = true;  // no data so continue
//                                }
//                                else if ((ibProviderError != null) && (ibProviderError.Code == 321))
//                                {
//                                    throw new System.Exception("IB Provider Error.  Please connect and retry.");
//                                }
//                                else if ((ibProviderError != null) && (ibProviderError.Code == 1100))
//                                {
//                                    throw new System.Exception("IB Provider Disconnect.  Please connect and retry.");
//                                }
//                                else if (ibProviderError != null)
//                                {
//                                    Console.WriteLine(DateTime.Now + " Unknown Provider Code " + ibProviderError.Code);
//                                }
//                                else
//                                {
//                                    done = true;
//                                }
//                            }
//                            else
//                            {
//                                done = true;
//                            }
//                        }
//                        while (!done);


//                        if (historicalBarsFoundOnlineInProvider.Count > 0)
//                            if (LoggingConfig.IsDebugEnabled) Console.WriteLine("First: \t" + historicalBarsFoundOnlineInProvider[0]);
//                        if (historicalBarsFoundOnlineInProvider.Count > 0)
//                            if (LoggingConfig.IsDebugEnabled) Console.WriteLine("Last: \t" + historicalBarsFoundOnlineInProvider[historicalBarsFoundOnlineInProvider.Count - 1]);


//                        AddCount = 0;

//                        // if not the same number of bars in database, then try to add the new ones
//                        if (historicalBarsFoundInLocalDb != historicalBarsFoundOnlineInProvider.Count)
//                            for (int i = 0; i < historicalBarsFoundOnlineInProvider.Count; i++)
//                            {
//                                currentBarToBeAdded = historicalBarsFoundOnlineInProvider[i];
//                                if (DataManager.GetHistoricalBars(Instrument, (currentBarToBeAdded.DateTime), (currentBarToBeAdded.DateTime).AddMilliseconds(1), currentBarRequest.BarType, currentBarRequest.BarSize).Count != 1)
//                                {
//                                    if (((currentBarToBeAdded.DateTime).TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds) && ((currentBarToBeAdded.DateTime).TimeOfDay.TotalSeconds < PstSessionTimeConstants.StockExchangeEndTimeSeconds))
//                                    {
//                                        if (Bid_Ask)
//                                        {
//                                            DataManager.Add(Instrument, (currentBarToBeAdded.DateTime), (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (int)(-1), currentBarToBeAdded.Size);	// add bar from BID/ASK data
//                                            DataManager.Add(Instrument, (currentBarToBeAdded.DateTime), currentBarToBeAdded.Open, (int)(-1), currentBarToBeAdded.Close, (int)(-1));				// assume bar is BID_ASK and add as quote
//                                        }
//                                        else
//                                        {
//                                            DataManager.Add(Instrument, (currentBarToBeAdded.DateTime), currentBarToBeAdded.Open, currentBarToBeAdded.High, currentBarToBeAdded.Low,
//                                                            currentBarToBeAdded.Close, currentBarToBeAdded.Volume, currentBarToBeAdded.Size); // add bar
//                                        }
//                                        AddCount++;
//                                        InstrumentAddCount++;
//                                        allInstrumentTotalCount++;
//                                    }
//                                }
//                            }
//                        if (LoggingConfig.IsDebugEnabled) Console.WriteLine(Instrument + " Added: \t" + AddCount);
//                    }
//                    tempDateCounterStart = tempDateCounterStart.AddSeconds(numberOfRequiredBars * currentBarRequest.BarSize);
//                }
//                else
//                {
//                    tempDateCounterStart = tempDateCounterStart.AddSeconds(currentBarRequest.BarSize);
//                }
//            }
//            return InstrumentAddCount;
//        }

//        private int ProcessHistoricalBarRequest2(BarRequest currentBarRequest, int InstrumentAddCount)
//        {
//            DateTime tempDateCounterStart;
//            int NumberOfBars;
//            DateTime tempDateCounterEnd;
//            int historicalBarsFoundInLocalDb;
//            BarSeries historicalBarsFoundOnlineInProvider;
//            Bar currentBarToBeAdded;
//            int AddCount;

//            if (LoggingConfig.IsDebugEnabled)
//                LoggingUtility.WriteDebug(LoggingConfig,
//                                          string.Format(
//                                              "Processing historical bar request. BarSize: {0}, BarType: {1}",
//                                              currentBarRequest.BarSize, currentBarRequest.BarType));

//            switch (currentBarRequest.BarSize)
//            {
//                case 1:
//                    NumberOfBars = 30 * 60;
//                    break;
//                case PeriodConstants.PERIOD_MINUTE:
//                    NumberOfBars = (int)((PstSessionTimeConstants.StockExchangeEndTimeSeconds - PstSessionTimeConstants.StockExchangeStartTimeSeconds) / 60.0);
//                    break;
//                default:
//                    throw new System.Exception("Unknown Step Size");
//            }
//            tempDateCounterStart = StartDate;

//            while (tempDateCounterStart < StopDate && tempDateCounterStart < DateTime.Now)
//            {
//                if (ibProviderDisconnect)
//                    throw new System.Exception("IB Provider Disconnect.  Please connect and retry.");

//                if (tempDateCounterStart.DayOfWeek == DayOfWeek.Saturday)
//                    tempDateCounterStart = tempDateCounterStart.AddDays(2);

//                if (tempDateCounterStart.DayOfWeek == DayOfWeek.Sunday)
//                    tempDateCounterStart = tempDateCounterStart.AddDays(1);

//                // if((D.TimeOfDay>=StartTime)&&(D.TimeOfDay<EndTime)&&(D.Date==D.AddSeconds(NumberOfBars*BR.BarSize).Date)) {
//                if ((tempDateCounterStart.TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds) && (tempDateCounterStart.TimeOfDay.TotalSeconds < PstSessionTimeConstants.StockExchangeEndTimeSeconds))
//                {
//                    Console.WriteLine(DateTime.Now + " Processing " + Instrument.ToString() + " " + tempDateCounterStart);

//                    tempDateCounterEnd = tempDateCounterStart.AddSeconds(NumberOfBars * currentBarRequest.BarSize);
//                    if (tempDateCounterEnd > (tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds)))
//                        tempDateCounterEnd = tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds);

//                    if (LoggingConfig.IsDebugEnabled) Console.WriteLine("Looking from\t" + tempDateCounterStart);
//                    if (LoggingConfig.IsDebugEnabled) Console.WriteLine("Looking to  \t" + tempDateCounterEnd);

//                    historicalBarsFoundInLocalDb = DataManager.GetHistoricalBars(Instrument, tempDateCounterStart, tempDateCounterEnd.AddSeconds(-1), currentBarRequest.BarType, currentBarRequest.BarSize).Count;
//                    if (LoggingConfig.IsDebugEnabled) Console.WriteLine("DataManager DMFound \t" + historicalBarsFoundInLocalDb);

//                    if (historicalBarsFoundInLocalDb < NumberOfBars)
//                    {
//                        bool done = false;
//                        do
//                        {
//                            TimeSpan t = DateTime.Now.Subtract(LastTime);
                            
//                            // Delay by at least 2 seconds so not to overload HMDS server
//                            if (t.TotalMilliseconds < 2000)
//                                System.Threading.Thread.Sleep(2000 - (int)t.TotalMilliseconds);

//                            if ((RequestCount++) == (Bid_Ask ? 30 : 60))
//                            {
//                                t = DateTime.Now.Subtract(FirstTime);
//                                // if 60 request and been less than 10 min, go to sleep until past 10min
//                                // so not to get pacing violation
//                                if (t.TotalSeconds < 10 * 60)
//                                {
//                                    Console.WriteLine(DateTime.Now + "\tGoing to Sleep.");
//                                    TimeSpan SleepTime = new TimeSpan(0, 10, 30) - t;
//                                    Console.WriteLine("Sleeping for " + SleepTime);
//                                    System.Threading.Thread.Sleep(SleepTime);
//                                    Console.WriteLine(DateTime.Now + "\tWaking up.");
//                                }
//                                RequestCount = 0;
//                                FirstTime = DateTime.Now;
//                            }
//                            LastTime = DateTime.Now;
//                            ibProviderError = null;

//                            tempDateCounterEnd = tempDateCounterStart.AddSeconds(NumberOfBars * currentBarRequest.BarSize);
//                            if (tempDateCounterEnd > (tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds)))
//                                tempDateCounterEnd = tempDateCounterStart.Date.AddSeconds(PstSessionTimeConstants.StockExchangeEndTimeSeconds);

//                            historicalBarsFoundOnlineInProvider = DataManager.GetHistoricalBars("IB", Instrument, tempDateCounterStart, tempDateCounterEnd, (int)currentBarRequest.BarSize);
//                            if (LoggingConfig.IsDebugEnabled) Console.WriteLine(DateTime.Now + " IB Returned Count " + historicalBarsFoundOnlineInProvider.Count);

//                            if (historicalBarsFoundOnlineInProvider.Count <= 0)
//                            {
//                                for (int i = 0; (i < 10) && (ibProviderError == null); i++)
//                                {
//                                    System.Threading.Thread.Sleep(100);
//                                }

//                                if ((ibProviderError != null) && (ibProviderError.Code == 162) && (ibProviderError.Message.Contains("pacing")))
//                                {
//                                    Console.WriteLine(DateTime.Now + "\tPacing Violation.  Going to Sleep.");
//                                    // sleep for over 10 minutes to insure HMDS server is reset
//                                    TimeSpan SleepTime = new TimeSpan(0, 10, 30);
//                                    Console.WriteLine("Sleeping for " + SleepTime);
//                                    System.Threading.Thread.Sleep(SleepTime);
//                                    Console.WriteLine(DateTime.Now + "\tWaking up.  Try Again.");
//                                    RequestCount = 0;
//                                    FirstTime = DateTime.Now;
//                                }
//                                else if ((ibProviderError != null) && (ibProviderError.Code == 162) && (ibProviderError.Message.Contains("no data")))
//                                {
//                                    done = true;  // no data so continue
//                                }
//                                else if ((ibProviderError != null) && (ibProviderError.Code == 321))
//                                {
//                                    throw new System.Exception("IB Provider Error.  Please connect and retry.");
//                                }
//                                else if ((ibProviderError != null) && (ibProviderError.Code == 1100))
//                                {
//                                    throw new System.Exception("IB Provider Disconnect.  Please connect and retry.");
//                                }
//                                else if (ibProviderError != null)
//                                {
//                                    Console.WriteLine(DateTime.Now + " Unknown Provider Code " + ibProviderError.Code);
//                                }
//                                else
//                                {
//                                    done = true;
//                                }
//                            }
//                            else
//                            {
//                                done = true;
//                            }
//                        } 
//                        while (!done);


//                        if (historicalBarsFoundOnlineInProvider.Count > 0)
//                            if (LoggingConfig.IsDebugEnabled) Console.WriteLine("First: \t" + historicalBarsFoundOnlineInProvider[0]);
//                        if (historicalBarsFoundOnlineInProvider.Count > 0)
//                            if (LoggingConfig.IsDebugEnabled) Console.WriteLine("Last: \t" + historicalBarsFoundOnlineInProvider[historicalBarsFoundOnlineInProvider.Count - 1]);


//                        AddCount = 0;

//                        // if not the same number of bars in database, then try to add the new ones
//                        if (historicalBarsFoundInLocalDb != historicalBarsFoundOnlineInProvider.Count)
//                            for (int i = 0; i < historicalBarsFoundOnlineInProvider.Count; i++)
//                            {
//                                currentBarToBeAdded = historicalBarsFoundOnlineInProvider[i];
//                                if (DataManager.GetHistoricalBars(Instrument, (currentBarToBeAdded.DateTime), (currentBarToBeAdded.DateTime).AddMilliseconds(1), currentBarRequest.BarType, currentBarRequest.BarSize).Count != 1)
//                                {
//                                    if (((currentBarToBeAdded.DateTime).TimeOfDay.TotalSeconds >= PstSessionTimeConstants.StockExchangeStartTimeSeconds) && ((currentBarToBeAdded.DateTime).TimeOfDay.TotalSeconds < PstSessionTimeConstants.StockExchangeEndTimeSeconds))
//                                    {
//                                        if (Bid_Ask)
//                                        {
//                                            DataManager.Add(Instrument, (currentBarToBeAdded.DateTime), (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (currentBarToBeAdded.Open + currentBarToBeAdded.Close) / 2, (int)(-1), currentBarToBeAdded.Size);	// add bar from BID/ASK data
//                                            DataManager.Add(Instrument, (currentBarToBeAdded.DateTime), currentBarToBeAdded.Open, (int)(-1), currentBarToBeAdded.Close, (int)(-1));				// assume bar is BID_ASK and add as quote
//                                        }
//                                        else
//                                        {
//                                            DataManager.Add(Instrument, (currentBarToBeAdded.DateTime), currentBarToBeAdded.Open, currentBarToBeAdded.High, currentBarToBeAdded.Low,
//                                                            currentBarToBeAdded.Close, currentBarToBeAdded.Volume, currentBarToBeAdded.Size); // add bar
//                                        }
//                                        AddCount++;
//                                        InstrumentAddCount++;
//                                        allInstrumentTotalCount++;
//                                    }
//                                }
//                            }
//                        if (LoggingConfig.IsDebugEnabled) Console.WriteLine(Instrument + " Added: \t" + AddCount);
//                    }
//                    tempDateCounterStart = tempDateCounterStart.AddSeconds(NumberOfBars * currentBarRequest.BarSize);
//                }
//                else
//                {
//                    tempDateCounterStart = tempDateCounterStart.AddSeconds(currentBarRequest.BarSize);
//                }
//            }
//            return InstrumentAddCount;
//        }

//        //    public override void OnError(ProviderError error)
//    //    {
//    //        if (error.Provider == "IB")
//    //        {
//    //            IBProviderError = error;
//    //            if (error.Code == 1100)
//    //                IBProviderDisconnect = true;
//    //            if (error.Code > 0)
//    //                if (LoggingConfig.IsDebugEnabled)
//    //                    Console.WriteLine(DateTime.Now.ToString() + " Provider: " + error.Provider + " Provider Code: " + error.Code + "\tProvider Message: " + error.Message);
//    //        }
//    //    }



//    }
//}