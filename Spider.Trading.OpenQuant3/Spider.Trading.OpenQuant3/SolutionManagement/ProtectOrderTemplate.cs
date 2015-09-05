using OpenQuant.API;

namespace Spider.Trading.OpenQuant3.SolutionManagement
{
    public class ProtectOrderTemplate : BaseTemplate
    {



        public static ProtectOrderTemplate AtOpenOrder
        {
            get
            {
                return new ProtectOrderTemplate()
                    .AtOpen() 
                    .WithStopTypeOfProtectiveStop()
                    as ProtectOrderTemplate;
            }
        }

        public static ProtectOrderTemplate AtCloseOrder
        {
            get { return new ProtectOrderTemplate()
                .AtClose()
                .WithStopTypeOfProtectiveStop() 
                as ProtectOrderTemplate;
            }
        }

        public static ProtectOrderTemplate IntradayOrder
        {
            get { return new ProtectOrderTemplate()
                .Intraday()
                .WithStopTypeOfProtectiveStop() 
                as ProtectOrderTemplate;
            }
        }

        protected override OrderSide OrderSide
        {
            get { return OrderSide.Sell; }
        }
    }
}