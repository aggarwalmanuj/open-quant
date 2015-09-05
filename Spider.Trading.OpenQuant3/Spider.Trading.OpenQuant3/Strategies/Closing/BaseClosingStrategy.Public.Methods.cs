//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using OpenQuant.API;
//using Spider.Trading.OpenQuant3.Diagnostics;
//using Spider.Trading.OpenQuant3.Util;

//namespace Spider.Trading.OpenQuant3.Strategies.Closing
//{
//    public abstract partial class BaseClosingStrategy
//    {
//        private PositionSizeHolder posSizeMgr;

//        protected void AddPositionSize(string symbol, PositionSide positionSide, double positionSize)
//        {
//            if (string.Compare(Instrument.Symbol, symbol, StringComparison.InvariantCultureIgnoreCase) == 0)
//            {
//                double qty = (positionSide == PositionSide.Long) ? Math.Abs(positionSize) : Math.Abs(positionSize) * -1;
//                PositionSizeManager.AddPositionSize(symbol, qty);

//                LoggingUtility.WriteDebug(LoggingConfig, string.Format("Added a position size of {0}", positionSize));
//            }
//        }

//        protected PositionSizeHolder PositionSizeManager
//        {
//            get
//            {
//                if (null == posSizeMgr)
//                {
//                    lock (LockObjectManager.LockObject)
//                    {
//                        if (null == posSizeMgr)
//                            posSizeMgr = new PositionSizeHolder();
//                    }
//                }
//                return posSizeMgr;
//            }
//        }

//    }
//}
