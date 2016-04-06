using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monica.Common.Utils;

namespace Monica.Quant
{
    public enum StrategyType
    {
        Intraday,
        Interday
    }

   public class StrategyConfig
    {
        public string Name { get; set; }

        public StrategyType StrategyType { get; set; }

        public Dictionary<string,double> Args { get; set; }

        public List<string> ProductInfos { get; set; } 

        public double InitX { get; set; }
    }
}
