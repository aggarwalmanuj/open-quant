using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies
{

   
    public abstract class BaseOpeningStrategyOld : BaseStrategyOld
    {
        
        [Parameter("Order Side", "Other Options")]
        public OrderSide OrderType = OrderSide.Sell;

        [Parameter("Round Lots", "Other Options")]
        public bool RoundLots = false;

        protected DateTime? triggerTime = null;

        protected Order openingOrder = null;


        /// <summary>
        /// 
        /// </summary>
        protected virtual void HandleStrategyStart()
        {
            
            triggerTime = TiggerOrderDate.Date.AddHours(TriggerOrderHour).AddMinutes(TriggerOrderMinute);

            if (LogOutput)
                Console.WriteLine(string.Format("Opening order trigger queued for {0} @ {1}", Instrument.Symbol, triggerTime.Value));

            PreLoadBarData(Instrument);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        protected virtual void HandleBarOpen(Bar bar)
        {
            if (bar.BeginTime >= triggerTime)
            {
                if (!HasPosition && openingOrder == null)
                {
                    if (LogOutput)
                        Console.WriteLine(string.Format("Bar for {0} arrived at {1} with opening price @ {2:C}", Instrument.Symbol, bar.BeginTime, bar.Open));

                    double targetPrice = GetTargetPrice(bar, OrderType);
                    double targetQuantity = GetTargetQuantity(targetPrice, RoundLots);

                    targetPrice = RoundPrice(targetPrice);
                    openingOrder = LimitOrder(OrderType, targetQuantity, targetPrice, "Auto opening order");

                    LogOrder("Opening", Instrument.Symbol, OrderType, targetQuantity, targetPrice);

                    if (AutoSubmit)
                        openingOrder.Send();
                }
            }
        }
    }
}
