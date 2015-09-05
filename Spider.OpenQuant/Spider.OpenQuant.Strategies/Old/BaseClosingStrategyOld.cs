using System;
using OpenQuant.API;

namespace Spider.OpenQuant.Strategies
{
    public abstract class BaseClosingStrategyOld : BaseStrategyOld
    {

        protected DateTime? triggerTime = null;
        protected double? longPositionQuantity = null;
        protected double? shortPositionQuantity = null;
        private double? openPrice = null;
        protected Order closingOrder = null;


        /// <summary>
        /// 
        /// </summary>
        public virtual void HandleStrategyStart()
        {
            string symbol = Instrument.Symbol;
            BrokerAccount IBaccount = DataManager.GetBrokerInfo("IB").Accounts[0];

            foreach (BrokerPosition currentBrokerPosition in IBaccount.Positions)
            {
                if (currentBrokerPosition.Symbol == symbol && currentBrokerPosition.InstrumentType == InstrumentType.Stock)
                {
                    longPositionQuantity = currentBrokerPosition.LongQty;
                    shortPositionQuantity = currentBrokerPosition.ShortQty;

                    triggerTime = TiggerOrderDate.Date.AddHours(TriggerOrderHour).AddMinutes(TriggerOrderMinute);

                    if (LogOutput)
                    {
                        Console.WriteLine(string.Format("Closing order trigger queued for {0} @ {1}",
                                                        currentBrokerPosition.Symbol, triggerTime.Value));
                    }

                    PreLoadBarData(Instrument);

                    break;
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="bar"></param>
        public virtual void HandleBarOpen(Bar bar)
        {
            if (bar.BeginTime >= triggerTime)
            {
                if (openPrice == null)
                {
                    openPrice = bar.Open;
                }

                if (!HasPosition && closingOrder == null)
                {
                    OrderSide? side = null;
                    double targetPrice = openPrice.Value;
                    double targetQuantity = 0;

                    if (longPositionQuantity.HasValue && longPositionQuantity.Value > 0)
                    {
                        side = OrderSide.Sell;
                        targetPrice = GetSlippageAdjustedPrice(openPrice.Value, side.Value);
                        targetQuantity = longPositionQuantity.Value;
                    }

                    if (shortPositionQuantity.HasValue && shortPositionQuantity.Value > 0)
                    {
                        side = OrderSide.Buy;
                        targetPrice = GetSlippageAdjustedPrice(openPrice.Value, side.Value);
                        targetQuantity = shortPositionQuantity.Value;
                    }

                    targetPrice = RoundPrice(targetPrice);

                    if (side.HasValue)
                    {
                        closingOrder = LimitOrder(side.Value, targetQuantity, targetPrice, "Auto closing order");
                        
                        LogOrder("Closing", Instrument.Symbol, side.Value, targetQuantity, targetPrice);

                        if (AutoSubmit)
                            closingOrder.Send();
                    }
                }
            }
        }
	
    }
}