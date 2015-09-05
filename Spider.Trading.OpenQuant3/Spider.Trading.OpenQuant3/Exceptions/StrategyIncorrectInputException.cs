using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spider.Trading.OpenQuant3.Exceptions
{
    public class StrategyIncorrectInputException : Exception
    {
        public StrategyIncorrectInputException(string message)
            : base(message)
        {

        }

        public StrategyIncorrectInputException(string parameterName, string message)
            : base(string.Format("{0}: {1}", parameterName, message))
        {

        }
    }
}
