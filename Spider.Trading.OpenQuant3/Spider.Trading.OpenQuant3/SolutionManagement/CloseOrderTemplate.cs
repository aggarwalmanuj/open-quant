using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public class CloseOrderTemplate : BaseTemplate
    {



        public static CloseOrderTemplate AtOpenOrder
        {
            get { return new CloseOrderTemplate().AtOpen() as CloseOrderTemplate; }
        }

        public static CloseOrderTemplate AtCloseOrder
        {
            get { return new CloseOrderTemplate().AtClose() as CloseOrderTemplate; }
        }

        public static CloseOrderTemplate IntradayOrder
        {
            get { return new CloseOrderTemplate().Intraday() as CloseOrderTemplate; }
        }

        protected override OrderSide OrderSide
        {
            get { return OrderSide.Sell; }
        }
    }
}