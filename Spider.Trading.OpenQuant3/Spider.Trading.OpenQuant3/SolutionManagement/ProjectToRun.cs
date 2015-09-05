using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using OpenQuant.API.Engine;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Util;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public class ProjectToRun
    {
        private readonly string defaultCurrency = "USD";
        private readonly string defaultExchange = "SMART";
        const string IwmStopPriceTriggerStrategy = "OrderTriggerStrategy";


        public string Currency { get; set; }

        public string Exchange { get; set; }

        private Dictionary<string, object> ProjectTemplateParameters;
        private Dictionary<string, object> SolutionWideParameters;

        private string stockSymbols;
        private int? executionHour;
        private int? executionMinute;
        private double? portfolioAmt;
        private double? portfolioAllocation;
        private int? noOfPositions;
        private int? noOfRetries;
        private int? retryInterval;
        private double? minSlippage;
        private double? maxSlippage;
        private double? stopTriggerAtr;
        private double? gapTriggerAtr;
        private double? maxPortfolioRisk;
        private double? maxPositionRisk;
        private string ibAccountNumber;
        private RiskAppetiteStrategy? riskAppetiteStrategy;

        private BaseTemplate ProjectTemplate { get; set; }
        public string ProjectName { get; private set; }
        
        public ProjectToRun(BaseTemplate projectTemplate)
        {
            WithProjectTemplate(projectTemplate);
        }

        private ProjectToRun()
        {}

        public static ProjectToRun NewInstance
        {
            get { return new ProjectToRun(); }
        }

        public ProjectToRun WithProjectTemplate(BaseTemplate projectTemplate)
        {
            ProjectTemplateParameters = projectTemplate.Generate();
            ProjectTemplate = projectTemplate;
            Currency = defaultCurrency;
            Exchange = defaultExchange;
            return this;
        }

        public ProjectToRun WithProjectIndex(int index)
        {
            if (ProjectTemplate is CloseOrderTemplate)
                ProjectName = Enum.Parse(typeof (CloseOrderProjectNames), "Close_" + index.ToString("00")).ToString();
            else if (ProjectTemplate is OpenLongTemplate)
                ProjectName = Enum.Parse(typeof (OpenOrderProjectNames), "Open_" + index.ToString("00")).ToString();
            else if (ProjectTemplate is OpenShortTemplate)
                ProjectName = Enum.Parse(typeof (OpenOrderProjectNames), "Open_" + index.ToString("00")).ToString();
            else if (ProjectTemplate is ProtectOrderTemplate)
                ProjectName = Enum.Parse(typeof(ProtectOrderProjectNames), "Protect_" + index.ToString("00")).ToString();

            return this;
        }

        public ProjectToRun WithSolutionParameters(Dictionary<string, object> solutionWideParameters)
        {
            this.SolutionWideParameters = solutionWideParameters;
            return this;
        }

        public  ProjectToRun WithStockSymbols(string symbols)
        {
            this.stockSymbols = symbols;
            return this;
        }

        public ProjectToRun AtHour(int hour)
        {
            this.executionHour = hour;
            return this;
        }

        public ProjectToRun AtMinute(int minute)
        {
            this.executionMinute = minute;
            return this;
        }

        public ProjectToRun WithPortfolioRiskPercentage(double risk)
        {
            this.maxPortfolioRisk = risk;
            return this;
        }

        public ProjectToRun WithPositionRiskPercentage(double risk)
        {
            this.maxPositionRisk = risk;
            return this;
        }

        public ProjectToRun WithRishAppetiteStrategy(RiskAppetiteStrategy riskStrategy)
        {
            this.riskAppetiteStrategy = riskStrategy;
            return this;
        }


        public ProjectToRun WithAmount(double amount)
        {
            this.portfolioAmt = amount;
            return this;
        }

        public ProjectToRun WithPositionSize(double positionSize)
        {
            this.portfolioAllocation = positionSize;
            return this;
        }

        public ProjectToRun WithNoOfPositions(int positions)
        {
            this.noOfPositions = positions;
            return this;
        }

        public ProjectToRun WithNoORetries(int retries)
        {
            this.noOfRetries = retries;
            return this;
        }


        public ProjectToRun WithRetryInterval(int interval)
        {
            this.retryInterval = interval;
            return this;
        }

        public ProjectToRun WithMinSlippage(double slippage)
        {
            this.minSlippage = slippage;
            return this;
        }

        public ProjectToRun WithMaxSlippage(double slippage)
        {
            this.maxSlippage = slippage;
            return this;
        }

        public ProjectToRun WithStopTriggerAtr(double atr)
        {
            this.stopTriggerAtr = atr;
            return this;
        }

        public ProjectToRun WithGapTriggerAtr(double atr)
        {
            this.gapTriggerAtr = atr;
            return this;
        }


        public void SetupProject(Project prj)
        {
            SetupInstruments(prj);
            SetupParams(prj, ProjectTemplateParameters, false);
            SetupParams(prj, SolutionWideParameters, false);

            var localParamsDictionary = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

            if (executionHour.HasValue)
                ParamsSetterUtility.SetExecutionHour(localParamsDictionary, executionHour.Value);

            if (executionMinute.HasValue)
                ParamsSetterUtility.SetExecutionMinute(localParamsDictionary, executionMinute.Value);

            if (portfolioAmt.HasValue)
                localParamsDictionary.Add("GrandTotalPortfolioAmount", portfolioAmt.Value);

            if (portfolioAllocation.HasValue)
                localParamsDictionary.Add("PortfolioAllocationPercentage", portfolioAllocation.Value);

            if (noOfPositions.HasValue)
                localParamsDictionary.Add("NumberOfPortfolioPositions", noOfPositions.Value);

            if (maxPortfolioRisk.HasValue)
                localParamsDictionary.Add("MaxPortfolioRisk", maxPortfolioRisk.Value);

            if (maxPositionRisk.HasValue)
                localParamsDictionary.Add("MaxPositionRisk", maxPositionRisk.Value);

            if (riskAppetiteStrategy.HasValue)
                localParamsDictionary.Add("RiskAppetiteStrategy", riskAppetiteStrategy.Value);


         

            if (noOfRetries.HasValue)
            {
                if (noOfRetries.Value <= 0)
                {
                    localParamsDictionary.Add("OrderRetrialStrategy", OrderRetrialStrategy.None);
                    localParamsDictionary.Add("MaximumRetries", 0);
                }
                else
                {

                    localParamsDictionary.Add("MaximumRetries", noOfRetries.Value);
                }
            }

            if (retryInterval.HasValue)
                localParamsDictionary.Add("RetryTriggerIntervalMinute", retryInterval.Value);

            if(minSlippage.HasValue)
                localParamsDictionary.Add("MinAllowedSlippage", minSlippage.Value);

            if (maxSlippage.HasValue)
                localParamsDictionary.Add("MaxAllowedSlippage", maxSlippage.Value);

            if (stopTriggerAtr.HasValue)
            {
                localParamsDictionary.Add("IwmCalculatedStopAtrCoefficient", stopTriggerAtr.Value);
                localParamsDictionary.Add("CalculatedStopAtrCoefficient", stopTriggerAtr.Value);
            }
            if (gapTriggerAtr.HasValue)
            {
                localParamsDictionary.Add("IwmOpeningGapAtrCoefficient", gapTriggerAtr.Value);
                localParamsDictionary.Add("OpeningGapAtrCoefficient", gapTriggerAtr.Value);
            }


            if (SolutionWideParameters.ContainsKey("IbBrokerAccountNumber"))
            {
                object tempAccountNumber = SolutionWideParameters["IbBrokerAccountNumber"].ToString();
                if (null != tempAccountNumber)
                {
                    ibAccountNumber = tempAccountNumber.ToString();
                }
            }

            if (string.IsNullOrWhiteSpace(ibAccountNumber))
            {
                throw new ArgumentNullException("IbBrokerAccountNumber", "!!!! IB ACCOUNT NUMBER NOT PROVIDED. !!!!");

            }

            SetupClosingProjectParameters(prj, ibAccountNumber);
            SetupIwmForStopTriggerIfRequired(prj);
            
            SetupParams(prj, localParamsDictionary, true);
        }

        


        private void SetupParams(Project prj, Dictionary<string, object> paramsDictionary, bool logValues)
        {
            foreach (string currentParamName in paramsDictionary.Keys)
            {
                object val = paramsDictionary[currentParamName];
                if (null != val && prj.Parameters.Contains(currentParamName))
                {
                    SetupParamValue(prj, currentParamName, val, logValues);
                }
            }
        }


        private void SetupIwmForStopTriggerIfRequired(Project prj)
        {
            Parameter iwmStopPriceParam = null;
            if (prj.Parameters.Contains(IwmStopPriceTriggerStrategy))
            {
                iwmStopPriceParam = prj.Parameters[IwmStopPriceTriggerStrategy];
                if (null != iwmStopPriceParam)
                {
                    OrderTriggerStrategy ordStrat = (OrderTriggerStrategy)Enum.Parse(typeof(OrderTriggerStrategy), iwmStopPriceParam.Value.ToString(), true);

                    bool isBasedOnIwmStopPrice = ordStrat == OrderTriggerStrategy.IwmStopPriceBasedTrigger;
                    bool isIwmAlreadyAttached = false;

                    foreach (Instrument currentIns in prj.Instruments)
                    {
                        if (SolutionUtility.IsInstrumentIwm(currentIns))
                        {
                            isIwmAlreadyAttached = true;
                            break;
                        }
                    }

                    if (isBasedOnIwmStopPrice)
                    {
                        if (!isIwmAlreadyAttached)
                        {
                            AttachInstrument(prj, "IWM");
                            SetupParamValue(prj, "IsIwmAlsoToBeIncludedInOrder", false, false);
                        }
                        else
                        {
                            SetupParamValue(prj, "IsIwmAlsoToBeIncludedInOrder", true, false);
                        }
                    }
                }
            }
        }


        private void SetupClosingProjectParameters(Project prj, string ibAccountNumber)
        {
            if (prj.Name.StartsWith("CLOSE", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (Instrument currentIns in prj.Instruments)
                {
                    if (!SolutionUtility.IsInstrumentIwm(currentIns))
                    {
                        OrderSide ordSide = SolutionUtility.GetOrderSideToCloseCurrentPosition(currentIns.Symbol, ibAccountNumber);
                        SetupParamValue(prj, "OrderSideForIwmStopTriggerForClosing", ordSide, true);

                        break;
                    }
                }
            }
        }


        private void SetupParamValue(Project prj, string paramName, object val, bool logValue)
        {
            if (prj.Parameters.Contains(paramName))
            {
                if (logValue)
                    Console.WriteLine(string.Format("PARAM : {0} [VALUE: {1}]", paramName, val));
                prj.Parameters[paramName].Value = val;
            }
        }


        private void SetupInstruments(Project prj)
        {
            string[] symbols = stockSymbols.Split(new char[] { '\n', '\r' });
            prj.ClearInstruments();

            foreach (string symbol in symbols)
            {
                AttachInstrument(prj, symbol);
            }
        }


        private void AttachInstrument(Project prj, string symbol)
        {
            if (!string.IsNullOrEmpty(symbol))
            {

                string trimmedSymbol = symbol.Trim();
                string normalizedSymbol = string.Empty;


                if (trimmedSymbol.Length > 0)
                {
                    normalizedSymbol = trimmedSymbol.Replace(".", "");
                    Instrument currentIns = InstrumentManager.Instruments[normalizedSymbol];


                    if (null == currentIns || currentIns.Type != InstrumentType.Stock)
                    {
                        Console.WriteLine("ADDING SYMBOL -- " + trimmedSymbol);
                        Instrument newInstrument = new Instrument(InstrumentType.Stock, normalizedSymbol);
                        newInstrument.Currency = defaultCurrency;
                        newInstrument.Exchange = defaultExchange;


                        int dotIdx = trimmedSymbol.IndexOf('.');
                        if (dotIdx > -1)
                        {
                            newInstrument.AltSource = "IB";
                            newInstrument.AltSymbol = trimmedSymbol.Replace(".", " ");
                        }
                    }


                    currentIns = InstrumentManager.Instruments[normalizedSymbol];
                    if (null != currentIns)
                        prj.AddInstrument(currentIns);

                }
            }
        }
	
    }
}
