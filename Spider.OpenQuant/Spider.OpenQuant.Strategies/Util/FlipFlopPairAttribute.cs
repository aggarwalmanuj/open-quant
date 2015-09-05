using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Spider.OpenQuant.Strategies.Entities;

namespace Spider.OpenQuant.Strategies.Util
{
    [AttributeUsage(AttributeTargets.Field)]
    public class FlipFlopPairDescriptionAttribute : Attribute
    {

        private static Dictionary<FlipFlopPair, FlipFlopPairDescriptionAttribute> flipFlopAttributeDictionary =
            new Dictionary<FlipFlopPair, FlipFlopPairDescriptionAttribute>();
        private static readonly object lockObject = new object();

        public FlipFlopPairDescriptionAttribute(string longSymbol, string shortSymbol)
        {
            LongSymbol = longSymbol;
            ShortSymbol = shortSymbol;
        }
             
        public string LongSymbol { get; set; }

        public string ShortSymbol { get; set; }

        public static FlipFlopPairDescriptionAttribute GetAttribute(FlipFlopPair pair)
        {
            FlipFlopPairDescriptionAttribute retVal = null;

            if (!flipFlopAttributeDictionary.ContainsKey(pair))
            {
                Type type = typeof (FlipFlopPair);

                foreach (FlipFlopPair enumValue in Enum.GetValues(type))
                {
                    FieldInfo fi = type.GetField((enumValue.ToString()));
                    FlipFlopPairDescriptionAttribute att =
                        fi.GetCustomAttributes(typeof(FlipFlopPairDescriptionAttribute), true).FirstOrDefault()
                        as FlipFlopPairDescriptionAttribute;
                    if (null != att)
                    {
                        flipFlopAttributeDictionary[enumValue] = att;
                    }
                }

            }

            retVal = flipFlopAttributeDictionary[pair];

            return retVal;
            
        }
    }
}
