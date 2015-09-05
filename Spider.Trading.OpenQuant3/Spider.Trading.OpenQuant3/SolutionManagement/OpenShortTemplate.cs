using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public class OpenShortTemplate : BaseTemplate
    {



        public static OpenShortTemplate AtOpenOrder
        {
            get { return new OpenShortTemplate().AtOpen() as OpenShortTemplate; }
        }

        public static OpenShortTemplate AtCloseOrder
        {
            get { return new OpenShortTemplate().AtClose() as OpenShortTemplate; }
        }

        public static OpenShortTemplate IntradayOrder
        {
            get { return new OpenShortTemplate().Intraday() as OpenShortTemplate; }
        }

        protected override OrderSide OrderSide
        {
            get { return OrderSide.Sell; }
        }
    }
}