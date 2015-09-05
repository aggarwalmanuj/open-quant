using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenQuant.API;
using Spider.Trading.OpenQuant3.Enums;

namespace Spider.Trading.OpenQuant3.Entities
{
    [Serializable]
    public class CurrentOpenPosition
    {
        public string Symbol;

        public InstrumentType InstrumentType;

        public LossBasedStopPriceReferenceStrategy LossBasedStopPriceReferenceStrategy
        {
            private get;
            set;
        }

        public double Quantity;

        public PositionSide PositionSide;

        public double PreviousDayPrice;

        public double CurrentDayOpenPrice;

        public double LastPrice;

        public double PurchasePrice
        {
            get
            {
                return LossBasedStopPriceReferenceStrategy == LossBasedStopPriceReferenceStrategy.CurrentDayOpen
                           ? CurrentDayOpenPrice
                           : PreviousDayPrice;
            }
        }

        public double ProfitLossPerUnit;

        public double TotalProfitLossAmount
        {
            get { return ProfitLossPerUnit*Quantity; }
        }


        public double TotalPurchaseAmount
        {
            get { return PurchasePrice * Quantity; }
        }


        public double TotalCurrentValueAmount
        {
            get { return LastPrice * Quantity; }
        }

        public double ProfitLossPercentage
        {
            get
            {
                if (PurchasePrice <= 0)
                    return 0;
                return ProfitLossPerUnit/PurchasePrice*100;
            }
        }
    }
}
