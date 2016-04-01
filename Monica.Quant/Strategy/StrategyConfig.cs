using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monica.Quant.Strategy
{
    public enum StrategyType
    {
        Intraday,
        Interday
    }

   public class StrategyConfig
    {
        public List<string> ProductInfos { get; set; }

        public StrategyType StrategyType { get; set; }

        public Dictionary<string,double> Args { get; set; } 
    }
}
