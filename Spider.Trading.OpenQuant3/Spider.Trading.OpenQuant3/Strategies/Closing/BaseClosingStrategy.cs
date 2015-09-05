using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Calculations;
using Spider.Trading.OpenQuant3.Diagnostics;
using Spider.Trading.OpenQuant3.Entities;
using Spider.Trading.OpenQuant3.Enums;
using Spider.Trading.OpenQuant3.Exceptions;
using Spider.Trading.OpenQuant3.Validators.Closing;

namespace Spider.Trading.OpenQuant3.Strategies.Closing
{
    public abstract partial class BaseClosingStrategy : BaseStrategy
    {
        protected override void HandleValidateInput()
        {
            this.StrategyTypeOpenClose = Enums.StrategyTypeOpenClose.Close;

            if (IsCurrentInstrumentIwmAndIgnorable())
                return;

            MultipleClosingStrategiesForSymbolValidator.ValidateClosingSymbol(this);

            bool isValid = ((longPositionQuantity.HasValue || shortPositionQuantity.HasValue) &&
                            (longPositionQuantity.Value != 0 || shortPositionQuantity.Value != 0));

            if (!isValid)
                throw new StrategyIncorrectInputException("Closing Quantity", "Closing Quantity is not correct");
        }

        protected override void HandleStrategyStarting()
        {
            QueueUpClosingOrder();
        }

        

        protected override void HandleBarOpened(Bar bar)
        {
            if (IsPortfolioOrPositionStopLossEnabled && null != ClosingStrategyPortfolioManager)
                ClosingStrategyPortfolioManager.UpdatePricesForPosition();
        }

        protected override void HandleOrderTriggered(Bar bar, double targetPrice)
        {
            double targetQuantity = 0;

            // While closing sometimes it places duplicate orders which triggers 
            // unwanted reverse positions

            RefreshOpenQuantity(true);

            if (GetAbsoluteOpenQuantity() <= 0)
                // This means the order was already processed by other thread
                return;

            targetQuantity = GetTargetQuantity();

            if (targetPrice <= 0 || targetQuantity <= 0)
                throw new ApplicationException(
                    string.Format("Invalid price of quantity calculated. Price {0:c}, Qty {1}", targetPrice,
                                  targetQuantity));

            string orderName = GetAutoPlacedOrderName(TargetOrderType, EffectiveOrderSide, "Auto-Closed",
                                                      Instrument.Symbol, EffectiveOrderRetriesConsumed, IbBrokerAccountNumber);

            base.PlaceTargetOrder(targetQuantity,
                                  targetPrice,
                                  orderName);

            LoggingUtility.LogOrder(LoggingConfig, orderName, EffectiveOrderSide, targetQuantity, targetPrice, EffectiveOrderRetriesConsumed);
        }

       
        protected override void HandleStrategyStarted()
        {
            if (IsPortfolioOrPositionStopLossEnabled)
                ClosingStrategyPortfolioManager = new ClosingStrategyPortfolioManager(this);
        }

       


    }
}
