using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Validators.Opening;

namespace Spider.Trading.OpenQuant3.Strategies.Opening
{
    public abstract partial class BaseOpeningStrategy : BaseStrategy
    {
        public double EffectiveAmountToInvest;
        public double EffectiveAmountToRisk;

       
        protected override void HandleValidateInput()
        {
            base.StrategyTypeOpenClose = Enums.StrategyTypeOpenClose.Open;
            OpeningInvestmentValidator.SetAndValidateValue(this);
        }

        protected override void HandleStrategyStarting()
        {
            
        }

       
        protected override void HandleBarOpened(Bar bar)
        {
            
        }

        protected override void HandleOrderTriggered(Bar bar, double targetPrice)
        {
            double targetQuantity = GetTargetQuantity(targetPrice);

            if (targetPrice <= 0 || targetQuantity <= 0)
                throw new ApplicationException(string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice, targetQuantity));
            
            string orderName = GetAutoPlacedOrderName(TargetOrderType, EffectiveOrderSide, "Auto-Opened", Instrument.Symbol, EffectiveOrderRetriesConsumed, IbBrokerAccountNumber);

            base.PlaceTargetOrder(targetQuantity,
                                  targetPrice,
                                  orderName);

            LoggingUtility.LogOrder(LoggingConfig, orderName, OrderSide, targetQuantity, targetPrice, EffectiveOrderRetriesConsumed);
        }

        protected override void HandleStrategyStarted()
        {
            
        }

        protected override OrderSide GetEffectiveOrderSide()
        {
            return this.OrderSide;
        }
    }
}
