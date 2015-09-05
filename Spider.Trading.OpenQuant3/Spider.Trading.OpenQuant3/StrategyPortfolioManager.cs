using System;
using System.Collections.Generic;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Common;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Strategies.Closing;
using Spider.Trading.OpenQuant3.Util;

namespace Spider.Trading.OpenQuant3
{
    public class ClosingStrategyPortfolioManager
    {
        private BaseClosingStrategy closeStrategy = null;
        private string positionKey = null;
        private CurrentOpenPosition currOpenPos = null;
        private DateTime lastTimePositionLogged = new DateTime(2000, 1, 1);

        internal ClosingStrategyPortfolioManager(BaseClosingStrategy strategy)
        {
            this.closeStrategy = strategy;
            positionKey = FormatKey(PortfolioManagementConstants.PORTFOLIO_POSITION_KEY, closeStrategy);
            if (IsPortfolioOrPositionStopLossEnabled)
                AddOpenPositionToPortfolio();
        }

        internal void UpdatePricesForPosition()
        {
            if (!IsPortfolioOrPositionStopLossEnabled)
                return;

            if (null == currOpenPos)
                throw new InvalidCastException("Did not find CurrentOpenPositin object for " + positionKey);

            bool doesAnyPriceNeedsToBeUpdated = (currOpenPos.PreviousDayPrice != closeStrategy.EffectivePreviousClosePrice)
                                                || (currOpenPos.CurrentDayOpenPrice != closeStrategy.EffectiveOpenPrice)
                                                || (currOpenPos.LastPrice != closeStrategy.EffectiveQuotePrice);

            if (doesAnyPriceNeedsToBeUpdated)
            {
                currOpenPos.PreviousDayPrice = closeStrategy.EffectivePreviousClosePrice;
                currOpenPos.CurrentDayOpenPrice = closeStrategy.EffectiveOpenPrice;
                currOpenPos.LastPrice = closeStrategy.EffectiveQuotePrice;

                LogPosition();

                HandlePositionPricesUpdated();
            }
        }

        internal bool GetWhetherPortfolioStopHasTriggered()
        {
            if (!IsPortfolioOrPositionStopLossEnabled)
                return false;

            bool stopLossMet = false;

            try
            {
                if (Strategy.Global.ContainsKey(PortfolioManagementConstants.CURRENT_PORTFOLIO_STOP_LOSS_MET_KEY))
                {
                    stopLossMet = Convert.ToBoolean(Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_STOP_LOSS_MET_KEY]);
                }
            }
            catch
            {
            }

            return stopLossMet;
        }

        internal bool GetWhetherPositionStopHasTriggered()
        {
            if (!IsPortfolioOrPositionStopLossEnabled)
                return false;
           
            if (!Strategy.Global.ContainsKey(positionKey))
                return false;

            string stopLossMetKey = FormatKey(PortfolioManagementConstants.PORTFOLIO_POSITION_STOP_LOSS_MET_KEY,
                                              closeStrategy);

            bool stopLossMet = false;

            try
            {
                if (Strategy.Global.ContainsKey(stopLossMetKey))
                {
                    stopLossMet = Convert.ToBoolean(Strategy.Global[stopLossMetKey]);
                }
            }
            catch
            {
            }

            return stopLossMet;
        }

        private void AddOpenPositionToPortfolio()
        {
            EnsurePositionTableIsAdded();

            if (!Strategy.Global.ContainsKey(positionKey))
            {
                lock (LockObjectManager.LockObject)
                {
                    if (!Strategy.Global.ContainsKey(positionKey))
                    {
                        currOpenPos = new CurrentOpenPosition()
                        {
                            Symbol = closeStrategy.Instrument.Symbol,
                            InstrumentType = closeStrategy.Instrument.Type,
                            Quantity = closeStrategy.GetAbsoluteOpenQuantity(),
                            PositionSide = closeStrategy.GetOpenPositionSide(),
                            PreviousDayPrice = closeStrategy.EffectivePreviousClosePrice,
                            CurrentDayOpenPrice = closeStrategy.EffectiveOpenPrice,
                            LastPrice = closeStrategy.EffectiveQuotePrice,
                            LossBasedStopPriceReferenceStrategy = closeStrategy.LossBasedStopPriceReferenceStrategy
                        };

                        Strategy.Global.Add(positionKey, currOpenPos);

                        LogPosition();

                        AddPositionKeyToPortfolioTable(positionKey);
                    }
                }
            }
            else
            {
                throw new ApplicationException(
                    string.Format(
                        "The symbol '{0}' has already been included in the portfolio. Please ensure you are not running multiple closing strategies for this symbol.",
                        closeStrategy.Instrument.Symbol));
            }
        }

        private void HandlePositionPricesUpdated()
        {
            if (!IsPortfolioOrPositionStopLossEnabled || null == currOpenPos)
                return;

            double purchasePrice = currOpenPos.PurchasePrice;
            double currentPrice = currOpenPos.LastPrice;

            double profitPerShare = currentPrice - purchasePrice;
            if (currOpenPos.PositionSide == PositionSide.Short)
            {
                if (currentPrice > purchasePrice)
                    //Loss
                    profitPerShare = Math.Abs(profitPerShare)*-1;
                else
                    profitPerShare = Math.Abs(profitPerShare);
            }

            bool isProfitDifferent = (currOpenPos.ProfitLossPerUnit != profitPerShare);

            if (isProfitDifferent)
            {
                currOpenPos.ProfitLossPerUnit = profitPerShare;

                UpdatePositionStopStatus();

                HandlePortfolioPositionPriceUpdated();
            }
        }

        private void UpdatePositionStopStatus()
        {
            if (!IsPortfolioOrPositionStopLossEnabled)
                return;
           
            if (GetWhetherPositionStopHasTriggered())
                // already triggered - no point in checking again
                return;

            bool stopLossMet = false;
            string lossKey = FormatKey(PortfolioManagementConstants.PORTFOLIO_POSITION_STOP_LOSS_MET_KEY, closeStrategy);
          
            double lossPercentage = currOpenPos.ProfitLossPercentage;
           
            if (lossPercentage < 0)
            {
                stopLossMet = LossStopLossMet(lossPercentage, closeStrategy.PositionLossBasedStopPercentageValue);
                if (stopLossMet)
                    LoggingUtility.WriteInfo(closeStrategy.LoggingConfig, "Stop los based on Position Loss was met.");
            }


            Strategy.Global[lossKey] = stopLossMet;
        }

        private void HandlePortfolioPositionPriceUpdated()
        {
            if (!IsPortfolioStopLossEnabled)
                return;

            IEnumerable<string> positionKeys = GetPositionKeysFromPortfolioTable();

            double totalPurchaseValue = 0;
            double totalCurrentValue = 0;
            double totalProfit = 0;

            lock(LockObjectManager.LockObject)
            {
                foreach (var currentPosKey in positionKeys)
                {
                    if (Strategy.Global.ContainsKey(currentPosKey))
                    {
                        CurrentOpenPosition currentOpenPosition = Strategy.Global[currentPosKey] as CurrentOpenPosition;
                        if (null != currentOpenPosition)
                        {
                            totalPurchaseValue += currentOpenPosition.TotalPurchaseAmount;
                            totalCurrentValue += currentOpenPosition.TotalCurrentValueAmount;
                            totalProfit += currentOpenPosition.TotalProfitLossAmount;
                        }
                    }
                }

                double totalProfitPercentage = totalProfit/totalPurchaseValue*100;

                Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_CURRENT_VALUE_KEY] = totalCurrentValue;
                Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PURCHASE_VALUE_KEY] = totalPurchaseValue;
                Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PROFIT_LOSS_AMOUNT_KEY] = totalProfit;
                Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PROFIT_LOSS_PERC_KEY] = totalProfitPercentage;

                LogPortfolio();
            }

            UpdatePortfolioStopStatus();
        }

        private void UpdatePortfolioStopStatus()
        {
            if (!IsPortfolioStopLossEnabled)
                return;

            if (!Strategy.Global.ContainsKey(PortfolioManagementConstants.CURRENT_PORTFOLIO_PROFIT_LOSS_PERC_KEY))
                return;

            if (GetWhetherPortfolioStopHasTriggered())
                // already triggered - no point in checking again
                return;

            bool stopLossMet = false;

            double lossPercentage = 0;

            try
            {
                lossPercentage = Convert.ToDouble(Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PROFIT_LOSS_PERC_KEY]);
            }
            catch
            {
            }

            if (lossPercentage < 0)
            {
                stopLossMet = LossStopLossMet(lossPercentage, closeStrategy.PortfolioLossBasedStopPercentageValue);
                if (stopLossMet)
                    LoggingUtility.WriteInfo(closeStrategy.LoggingConfig, "Stop los based on Portfolio Loss was met.");
            }

            Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_STOP_LOSS_MET_KEY] = stopLossMet;
        }

        private static bool LossStopLossMet(double lossPercentage, double allowedLossPercentage)
        {
            return lossPercentage < (Math.Abs(allowedLossPercentage)*-1);
        }

        private static string FormatKey(string formatString, BaseClosingStrategy strategy)
        {
            return string.Format(formatString, strategy.Instrument.Symbol, strategy.Instrument.Type);
        }

        private bool IsPortfolioStopLossEnabled
        {
            get { return closeStrategy.IsPortfolioStopLossEnabled; }
        }
       
        private bool IsPortfolioOrPositionStopLossEnabled
        {
            get { return closeStrategy.IsPortfolioOrPositionStopLossEnabled; }
        }

        private void LogPosition()
        {
            if (null != currOpenPos && DateTime.Now.Subtract(lastTimePositionLogged).TotalMinutes > 10)
            {
                LoggingUtility.WriteInfo(closeStrategy.LoggingConfig,
                                         string.Format(
                                             "Current position - [Qty: {0}; Purchase Price: {1:c}; Current Price: {2:c}; Profit Per Unit: {3:c}; Profit Percentage: {4:p}]",
                                             currOpenPos.Quantity,
                                             currOpenPos.PurchasePrice,
                                             currOpenPos.LastPrice,
                                             currOpenPos.ProfitLossPerUnit,
                                             currOpenPos.ProfitLossPercentage*100));

                lastTimePositionLogged = DateTime.Now;
            }
        }

        private void LogPortfolio()
        {
            DateTime lastTimePortfolioLogged = new DateTime(2000, 1, 1);
            string lastTimePortfolioLoggedKey = "lastTimePortfolioLogged";

            if (Strategy.Global.ContainsKey(lastTimePortfolioLoggedKey))
            {
                try
                {
                    lastTimePortfolioLogged = Convert.ToDateTime(Strategy.Global[lastTimePortfolioLoggedKey]);
                }
                catch
                {
                }
            }

            if (DateTime.Now.Subtract(lastTimePortfolioLogged).TotalMinutes > 10)
            {
                try
                {
                    LoggingUtility.WriteInfo(closeStrategy.LoggingConfig,
                                             string.Format(
                                                 "Current Portfolio - [Total Purchase Price: {0:c}; Current Value: {1:c}; Profit: {2:c}; Profit Percentage: {3:p}",
                                                 Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PURCHASE_VALUE_KEY],
                                                 Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_CURRENT_VALUE_KEY],
                                                 Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PROFIT_LOSS_AMOUNT_KEY],
                                                 Convert.ToDouble(Strategy.Global[PortfolioManagementConstants.CURRENT_PORTFOLIO_PROFIT_LOSS_PERC_KEY]) * 100));
                    Strategy.Global[lastTimePortfolioLoggedKey] = DateTime.Now;
                }
                catch
                {
                }
                
            }
        }

        #region Position Key Table Management

        private static void AddPositionKeyToPortfolioTable(string closingSymbolKey)
        {
            lock (LockObjectManager.LockObject)
            {
                (Strategy.Global[PortfolioManagementConstants.PORTFOLIO_POSITION_TABLE] as Dictionary<string, string>).
                    Add(closingSymbolKey,
                        closingSymbolKey);
            }
        }

        private static IEnumerable<string> GetPositionKeysFromPortfolioTable()
        {
            return (Strategy.Global[PortfolioManagementConstants.PORTFOLIO_POSITION_TABLE] as Dictionary<string, string>).Keys;
        }

        private static void EnsurePositionTableIsAdded()
        {
            if (!Strategy.Global.ContainsKey(PortfolioManagementConstants.PORTFOLIO_POSITION_TABLE))
            {
                lock (LockObjectManager.LockObject)
                {
                    if (!Strategy.Global.ContainsKey(PortfolioManagementConstants.PORTFOLIO_POSITION_TABLE))
                    {
                        Strategy.Global.Add(PortfolioManagementConstants.PORTFOLIO_POSITION_TABLE, new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase));
                    }
                }
            }
        }

        #endregion
    }
}
