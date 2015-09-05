using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MS.Internal.Xml.XPath;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.SolutionManagement;


namespace Spider.Trading.OpenQuant3.TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            List<ProjectToRun> projects = new List<ProjectToRun>();

            projects.Add(ProjectToRun.NewInstance.WithProjectTemplate(CloseOrderTemplate.AtCloseOrder).WithProjectIndex(1));
            projects.Add(ProjectToRun.NewInstance.WithProjectTemplate(OpenLongTemplate.AtCloseOrder).WithProjectIndex(2));
            projects.Add(ProjectToRun.NewInstance.WithProjectTemplate(OpenShortTemplate.AtCloseOrder).WithProjectIndex(3));


            RiskCalculator calculator = new RiskCalculator();
            RiskCalculationOutput output = calculator.Calculate(
                new RiskCalculationInput()
                {
                    TotalPortfolioAmount = 15000,
                    PortfolioAllocationPercentage = 100,
                    NumberOfPositions = 3,
                    MaxPortfolioRisk = 10,
                    MaxPositionRisk = 2
                });


            var answer = output.MaximumAllocatedPositionAmount;
        }
    }
}
