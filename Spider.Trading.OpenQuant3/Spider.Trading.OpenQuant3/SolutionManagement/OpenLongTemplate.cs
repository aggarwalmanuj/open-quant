using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public class OpenLongTemplate : BaseTemplate
    {


        public static OpenLongTemplate AtOpenOrder
        {
            get { return new OpenLongTemplate().AtOpen() as OpenLongTemplate; }
        }

        public static OpenLongTemplate AtCloseOrder
        {
            get { return new OpenLongTemplate().AtClose() as OpenLongTemplate; }
        }

        public static OpenLongTemplate IntradayOrder
        {
            get { return new OpenLongTemplate().Intraday() as OpenLongTemplate; }
        }


        protected override OrderSide OrderSide
        {
            get { return OrderSide.Buy; }
        }
    }
}