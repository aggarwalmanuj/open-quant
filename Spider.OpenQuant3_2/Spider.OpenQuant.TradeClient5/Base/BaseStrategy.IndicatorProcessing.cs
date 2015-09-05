using System.Collections.Generic;
using System.Linq;
using System.Text;

using OpenQuant.API;

using Spider.OpenQuant.TradeClient5.Util;

namespace Spider.OpenQuant.TradeClient5.Base
{

    public abstract partial class BaseStrategy
    {

        protected bool IsStochCrossUp { get; set; }

        protected bool IsStochCrossDn { get; set; }

        protected bool IsEmaCrossUp { get; set; }

        protected bool IsEmaCrossDn { get; set; }

        protected bool IsStochInOverBoughtMode { get; set; }

        protected bool IsStochInOverSoldMode { get; set; }

        protected bool IsEmaAlmostCrossUp { get; set; }

        protected bool IsEmaAlmostCrossDn { get; set; }

        private bool GetIsEmaAlmostInBullishMode(Bar bar)
        {
            bool originalValue = IsEmaAlmostCrossUp;

            double slow = SlowEmaIndicator.Last;
            double fast = FastEmaIndicator.Last;

            bool currentValue = (CurrentLastPrice > 0) &&
                                (CurrentLastPrice > slow) &&
                                (CurrentLastPrice > fast);


            if (originalValue != currentValue)
            {
                if (currentValue)
                {
                    WriteIndicatorSignal(bar, "EMA ALMOST X UP",
                        string.Format("FastEma={0:F4}, SlowEma={1:F4}, LastPrice={2:F4}", fast, slow, CurrentLastPrice));
                }
            }

            IsEmaAlmostCrossUp = currentValue;
            return IsEmaAlmostCrossUp;
        }

        private bool GetIsEmaAlmostInBearishMode(Bar bar)
        {
            bool originalValue = IsEmaAlmostCrossDn;

            double slow = SlowEmaIndicator.Last;
            double fast = FastEmaIndicator.Last;

            bool currentValue = (CurrentLastPrice > 0) &&
                                (CurrentLastPrice < slow) &&
                                (CurrentLastPrice < fast);


            if (originalValue != currentValue)
            {
                if (currentValue)
                {
                    WriteIndicatorSignal(bar, "EMA ALMOST X DN",
                        string.Format("FastEma={0:F4}, SlowEma={1:F4}, LastPrice={2:F4}", fast, slow, CurrentLastPrice));
                }
            }

            IsEmaAlmostCrossDn = currentValue;
            return IsEmaAlmostCrossDn;
        }


        private bool GetIsEmaInBullishMode(Bar bar)
        {
            bool originalEmaUp = IsEmaCrossUp;

            double slow = SlowEmaIndicator.Last;
            double fast = FastEmaIndicator.Last;

            bool currentEmaUp = fast > slow;


            if (originalEmaUp != currentEmaUp)
            {
                if (currentEmaUp)
                {
                    WriteIndicatorSignal(bar, "EMA X UP", string.Format("FastEma={0:F4}, SlowEma={1:F4}", fast, slow));

                    AdjustSpreadSteppingBasedOnIndicatorEvent(true, false, 2, "EMA X UP");
                }
            }

            IsEmaCrossUp = currentEmaUp;
            return IsEmaCrossUp;
        }


        private bool GetIsEmaInBearishMode(Bar bar)
        {
            bool originalEmaDn = IsEmaCrossDn;

            double slow = SlowEmaIndicator.Last;
            double fast = FastEmaIndicator.Last;

            bool currentEmaDn = fast < slow;

            if (originalEmaDn != currentEmaDn)
            {
                if (currentEmaDn)
                {

                    WriteIndicatorSignal(bar, "EMA X DN", string.Format("FastEma={0:F4}, SlowEma={1:F4}", fast, slow));

                    AdjustSpreadSteppingBasedOnIndicatorEvent(false, true, 2, "EMA X DN");

                }
            }

            IsEmaCrossDn = currentEmaDn;
            return IsEmaCrossDn;
        }



        private bool GetIsStochInBearishMode(Bar bar)
        {
            bool originalStochDn = IsStochCrossDn;

            double currentKValue = -1;
            double currentDValue = 1;
            try
            {
                currentKValue = KSlowIndicator.Last;
            }
            catch
            {
                currentKValue = -1;
            }

            try
            {
                currentDValue = DSlowIndicator.Last;
            }
            catch
            {
                currentDValue = -1;
            }

            bool currentStochDn = currentKValue < currentDValue;


            if (originalStochDn != currentStochDn)
            {
                if (currentStochDn)
                {
                    WriteIndicatorSignal(bar, "STO X DN",
                        string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));

                    AdjustSpreadSteppingBasedOnIndicatorEvent(false, true, 1, "STO X DN");
                }

            }

            IsStochCrossDn = currentStochDn;
            return IsStochCrossDn;
        }



        private bool GetIsStochInBullishMode(Bar bar)
        {
            bool originalStochUp = IsStochCrossUp;

            double currentKValue = -1;
            double currentDValue = 1;
            try
            {
                currentKValue = KSlowIndicator.Last;
            }
            catch
            {
                currentKValue = -1;
            }

            try
            {
                currentDValue = DSlowIndicator.Last;
            }
            catch
            {
                currentDValue = -1;
            }
            bool currentStochUp = currentKValue > currentDValue;

            if (originalStochUp != currentStochUp)
            {
                if (currentStochUp)
                {

                    WriteIndicatorSignal(bar, "STO X UP",
                        string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));

                    AdjustSpreadSteppingBasedOnIndicatorEvent(true, false, 1, "STO X UP");
                }


            }

            IsStochCrossUp = currentStochUp;
            return IsStochCrossUp;
        }


        private void WriteIndicatorSignal(Bar bar, string signal, string indicatorString)
        {
            if (IsCurrentOrderActive() && IsBarCloseEnoughForLogging(bar))
            {
                LoggingUtility.WriteInfoFormat(this, "*** {0} *** @ {1} ({2})",
                    signal,
                    GetCurrentDateTime(),
                    indicatorString);
            }
        }

        private bool IsNormalizedStochInBullishMode
        {
            get
            {
                return IsStochInOverBoughtMode || IsStochCrossUp;
            }
        }

        private bool IsNormalizedStochInBearishMode
        {
            get
            {
                return IsStochInOverSoldMode || IsStochCrossDn;
            }
        }

      

        private bool GetIsStochInObMode(Bar bar)
        {
            bool originalValue = IsStochInOverBoughtMode;

            double currentKValue = -1;
            double currentDValue = 1;
            try
            {
                currentKValue = KSlowIndicator.Last;
            }
            catch
            {
                currentKValue = -1;
            }

            try
            {
                currentDValue = DSlowIndicator.Last;
            }
            catch
            {
                currentDValue = -1;
            }

            bool currentValue = currentKValue >= OverboughtStochThreshold;

            if (currentValue != originalValue)
            {
                if (currentValue)
                {
                    WriteIndicatorSignal(bar, "STO X OB",
                        string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));
                }
            }

            IsStochInOverBoughtMode = currentValue;
            return IsStochInOverBoughtMode;
        }

       

        private bool GetIsStochInOsMode(Bar bar)
        {
            bool originalValue = IsStochInOverSoldMode;

            double currentKValue = -1;
            double currentDValue = 1;
            try
            {
                currentKValue = KSlowIndicator.Last;
            }
            catch
            {
                currentKValue = -1;
            }

            try
            {
                currentDValue = DSlowIndicator.Last;
            }
            catch
            {
                currentDValue = -1;
            }

            bool currentValue = (currentKValue <= OversoldStochThreshold) && (currentKValue > -1);



            if (currentValue != originalValue)
            {
                if (currentValue)
                {
                    WriteIndicatorSignal(bar, "STO X OS",
                        string.Format("K={0:F4}, D={1:F4}", currentKValue, currentDValue));

                }
            }

            IsStochInOverSoldMode = currentValue;
            return IsStochInOverSoldMode;
        }


        private bool ArePriceActionIndicatorsInFavorableMode(OrderSide orderAction)
        {
            bool areIndicatorsFavorable = false;

            bool isEmaFired = false;
            bool isStochFired = false;

            string expectingIndicatorMode = string.Empty;
            bool almostInd = false;
            if (orderAction == OrderSide.Buy)
            {
                expectingIndicatorMode = "Bullish";
                isEmaFired = IsEmaCrossUp && !IsEmaAlmostCrossDn;
                isStochFired = IsNormalizedStochInBullishMode;
                almostInd = IsEmaAlmostCrossDn;
                // We are buying - check for bullish signs
                areIndicatorsFavorable = (isEmaFired && isStochFired) ||
                                         (IsStochInOverBoughtMode && !IsEmaAlmostCrossDn) ||
                                         (IsStochInOverSoldMode && IsEmaAlmostCrossUp && !IsEmaAlmostCrossDn) ||
                                         (IsStochInOverSoldMode && isEmaFired);
            }
            else
            {
                expectingIndicatorMode = "Bearish";
                isEmaFired = IsEmaCrossDn && !IsEmaAlmostCrossUp;
                isStochFired = IsNormalizedStochInBearishMode;
                almostInd = IsEmaAlmostCrossUp;
                // We are selling - check for bullish signs
                areIndicatorsFavorable = (isEmaFired && isStochFired) ||
                                         (IsStochInOverSoldMode && !IsEmaAlmostCrossUp) ||
                                         (IsStochInOverBoughtMode && IsEmaAlmostCrossDn && !IsEmaAlmostCrossUp) ||
                                         (IsStochInOverBoughtMode && isEmaFired);
            }

            string emaPart = string.Format("EMA({0}x{1})=({2:F4}x{3:F4})={4}. ALMOST EMA={5}",
                FastMaPeriod,
                SlowMaPeriod,
                FastEmaIndicator.Last,
                SlowEmaIndicator.Last,
                isEmaFired.ToString().ToUpper(),
                almostInd.ToString().ToUpper());

            string stochPart = string.Format("STOCH({0},{1},{2})=(K={3:F4},D={4:F4})={5}",
                StochasticsKPeriod,
                StochasticsDPeriod,
                StochasticsSmoothPeriod,
                KSlowIndicator.Last,
                DSlowIndicator.Last,
                isStochFired.ToString().ToUpper());

            string logMessage = string.Format("INDICATORS (Expecting={0})={1} ({2}, {3})",
                expectingIndicatorMode,
                areIndicatorsFavorable.ToString().ToUpper(),
                emaPart,
                stochPart
                );

            if (areIndicatorsFavorable)
            {
                LoggingUtility.WriteInfo(this, logMessage);
            }
            else
            {
                WriteInfrequentDebugMessage(logMessage);
            }


            return areIndicatorsFavorable;
        }
    }
}
