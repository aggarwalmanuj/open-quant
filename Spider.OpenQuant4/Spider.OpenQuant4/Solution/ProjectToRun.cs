using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQuant.API;
using OpenQuant.API.Engine;
using Spider.OpenQuant4.Util;

namespace Spider.OpenQuant4.Solution
{
    public class ProjectToRun
    {
        private readonly string defaultCurrency = "USD";
        private readonly string defaultExchange = "SMART";

        private Dictionary<string, object> SolutionWideParameters;

        private string stockSymbols;
        private int? executionHour;
        private int? executionMinute;
        private double? portfolioAmt;
        private double? portfolioAllocation;
        private int? noOfPositions;
        private int? retryInterval;
        private double? minSlippage;
        private double? maxSlippage;
        private string ibAccountNumber;
        private int? projIndex;
        private int? fastMaPeriod;
        private int? slowMaPeriod;
        private int? specificSize;
        private PositionSide? specificPosSide;


        public ProjectTemplateType? ProjectTemplate { get; private set; }

        public string OQProjectName { get; private set; }

        private ProjectToRun()
        {

        }

        public static ProjectToRun OpenLongProject()
        {
            return GenerateProject(ProjectTemplateType.OpenLong);
        }

        public static ProjectToRun OpenShortProject()
        {
            return GenerateProject(ProjectTemplateType.OpenShort);
        }

        public static ProjectToRun CloseProject()
        {
            return GenerateProject(ProjectTemplateType.Close);
        }

        public static ProjectToRun OpenReverseProject()
        {
            return GenerateProject(ProjectTemplateType.ReverseOpen);
        }

        public static ProjectToRun CloseReverseProject()
        {
            return GenerateProject(ProjectTemplateType.ReverseClose);
        }

        public static ProjectToRun GenerateProject(ProjectTemplateType type)
        {
            ProjectToRun projToRun = new ProjectToRun();
            projToRun.ProjectTemplate = type;
            return projToRun;
        }

        public static ProjectToRun Clone(ProjectToRun origProject, ProjectTemplateType type)
        {
            ProjectToRun projToRun = new ProjectToRun();

            projToRun.stockSymbols = origProject.stockSymbols;
            projToRun.executionHour = origProject.executionHour;
            projToRun.executionMinute = origProject.executionMinute;
            projToRun.portfolioAmt = origProject.portfolioAmt;
            projToRun.portfolioAllocation = origProject.portfolioAllocation;
            projToRun.noOfPositions = origProject.noOfPositions;
            projToRun.retryInterval = origProject.retryInterval;
            projToRun.minSlippage = origProject.minSlippage;
            projToRun.maxSlippage = origProject.maxSlippage;
            projToRun.ibAccountNumber = origProject.ibAccountNumber;
            projToRun.projIndex = origProject.projIndex;
            projToRun.fastMaPeriod = origProject.fastMaPeriod;
            projToRun.slowMaPeriod = origProject.slowMaPeriod;
            projToRun.ProjectTemplate = type;
            projToRun.OQProjectName = origProject.OQProjectName;
            projToRun.specificSize = origProject.specificSize;
            projToRun.specificPosSide = origProject.specificPosSide;
            projToRun.SolutionWideParameters = origProject.SolutionWideParameters;

            return projToRun;
        }


        public ProjectToRun WithProjectIndex(int index)
        {

            projIndex = index;
            if (ProjectTemplate == ProjectTemplateType.Close)
                OQProjectName = Enum.Parse(typeof(CloseOrderProjectNames), "Close_" + index.ToString("00")).ToString();
            else if (ProjectTemplate == ProjectTemplateType.OpenLong)
                OQProjectName = Enum.Parse(typeof(OpenOrderProjectNames), "Open_" + index.ToString("00")).ToString();
            else if (ProjectTemplate == ProjectTemplateType.OpenShort)
                OQProjectName = Enum.Parse(typeof(OpenOrderProjectNames), "Open_" + index.ToString("00")).ToString();
            else if (ProjectTemplate == ProjectTemplateType.ReverseOpen)
                OQProjectName = Enum.Parse(typeof(ReverseOpenOrderProjectNames), "ReverseOpen_" + index.ToString("00")).ToString();
            else if (ProjectTemplate == ProjectTemplateType.ReverseClose)
                OQProjectName = Enum.Parse(typeof(ReverseCloseOrderProjectNames), "ReverseClose_" + index.ToString("00")).ToString();

            return this;
        }

        public ProjectToRun WithSolutionParameters(Dictionary<string, object> solutionWideParameters)
        {
            this.SolutionWideParameters = solutionWideParameters;
            return this;
        }

        public ProjectToRun WithSpecificSize(int size)
        {
            this.specificSize = size;
            return this;
        }

        public ProjectToRun WithSpecificPositionSideToReverse(PositionSide side)
        {
            this.specificPosSide = side;
            return this;
        }

        public ProjectToRun WithStockSymbols(string symbols)
        {
            this.stockSymbols = symbols.ToUpperInvariant();
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

        public ProjectToRun WithIbAccountNumber(string acctNumber)
        {
            this.ibAccountNumber = acctNumber;
            return this;
        }

        public ProjectToRun WithFastMaPeriod(int period)
        {
            this.fastMaPeriod = period;
            return this;
        }


        public ProjectToRun WithSlowMaPeriod(int period)
        {
            this.slowMaPeriod = period;
            return this;
        }


        public ProjectToRun WithPositiveEarningsNextDayOpen()
        {
            this.AtHour(6);
            this.AtHour(47);
            this.WithMinSlippage(-0.07);
            return this;
        }

        public ProjectToRun WithNegativeEarningsNextDayOpen()
        {
            this.AtHour(6);
            this.AtHour(30);
            this.WithMinSlippage(-0.03);
            return this;
        }

        public ProjectToRun WithNegativeEarningsCurrentDay()
        {
            this.WithMinSlippage(-0.03);
            return this;
        }

        public void SetupProject(Project prj)
        {
            SetupInstruments(prj);
            SetupParams(prj, SolutionWideParameters, false);


            //private int? retryInterval;
            //private double? minSlippage;
            //private double? maxSlippage;
            //private string ibAccountNumber;

            var localParamsDictionary = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);


            if (executionHour.HasValue)
                localParamsDictionary.Add("ValidityTriggerHour", executionHour.Value);

            if (executionMinute.HasValue)
                localParamsDictionary.Add("ValidityTriggerMinute", executionMinute.Value);

            if (portfolioAmt.HasValue)
                localParamsDictionary.Add("TotalPortfolioAmount", portfolioAmt.Value);

            if (portfolioAllocation.HasValue)
                localParamsDictionary.Add("PositionSizePercentage", portfolioAllocation.Value);

            if (noOfPositions.HasValue)
                localParamsDictionary.Add("NumberOfPortfolioPositions", noOfPositions.Value);

            if (retryInterval.HasValue)
                localParamsDictionary.Add("MaximumIntervalInMinutesBetweenOrderRetries", retryInterval.Value);

            if (minSlippage.HasValue)
                localParamsDictionary.Add("MinAllowedSlippage", minSlippage.Value);

            if (maxSlippage.HasValue)
                localParamsDictionary.Add("MaxAllowedSlippage", maxSlippage.Value);

            if (!string.IsNullOrWhiteSpace(ibAccountNumber))
                localParamsDictionary.Add("IbAccountNumber", ibAccountNumber);

            if (executionHour.HasValue)
                localParamsDictionary.Add("ValidityTriggerHour", executionHour.Value);

            if (fastMaPeriod.HasValue)
                localParamsDictionary.Add("FastMaPeriod", fastMaPeriod.Value);

            if (slowMaPeriod.HasValue)
                localParamsDictionary.Add("SlowMaPeriod", slowMaPeriod.Value);

            if (specificSize.HasValue)
                localParamsDictionary.Add("SpecificPositionSize", specificSize.Value);

            if (specificPosSide.HasValue)
                localParamsDictionary.Add("SpecificPositionSideToReverse", specificPosSide.Value);


            if (ProjectTemplate.Value == ProjectTemplateType.OpenLong)
            {
                localParamsDictionary.Add("OpeningOrderSide", OrderSide.Buy);
            }
            else if (ProjectTemplate.Value == ProjectTemplateType.OpenShort)
            {
                localParamsDictionary.Add("OpeningOrderSide", OrderSide.Sell);
            }
            string projNameForStrategy = "P00";
            if (projIndex.HasValue)
            {
                projNameForStrategy = ProjectTemplate.Value.ToString().ToUpper().Substring(0, 1) +
                                      projIndex.Value.ToString("00");
            }
            localParamsDictionary.Add("ProjectName", projNameForStrategy);

            SetupParams(prj, localParamsDictionary, true);

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

        private void SetupParamValue(Project prj, string paramName, object val, bool logValue)
        {
            if (prj.Parameters.Contains(paramName))
            {
                if (logValue)
                    Console.WriteLine(string.Format("PARAM : {0} [VALUE: {1}]", paramName, val));
                prj.Parameters[paramName].Value = val;
            }
        }


        private void AttachInstrument(Project prj, string symbol)
        {
            if (string.IsNullOrEmpty(symbol))
                return;

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
                        AltIDGroup ibAltIdGroup = newInstrument.AltIDGroups.Add("IB");
                        ibAltIdGroup.AltSource = "IB";
                        ibAltIdGroup.AltSymbol = trimmedSymbol.Replace(".", " ");
                    }
                }


                currentIns = InstrumentManager.Instruments[normalizedSymbol];
                if (null != currentIns)
                    prj.AddInstrument(currentIns);

            }
        }

    }

}

