using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Monica.Common.Pocos;

namespace Monica.Quant.Strategy
{

    

    public delegate List<Signal> BarDatasCallbackHandler(DateTime time, Dictionary<string, BarData> barDataDict);
    public abstract class Strategy
    {
        public Dictionary<DateTime,List<Signal>> SignalDict { get; }


        public Dictionary<DateTime,Dictionary<string,BarData>> BarDataDict { get; private set; } 
       
        protected StrategyConfig StrategyConfig;

        protected abstract List<Signal> OnBarDatas(DateTime time,Dictionary<string,BarData> barDataDict);

        protected Strategy(StrategyConfig config)
        {
            StrategyConfig = config;
            SignalDict = new Dictionary<DateTime, List<Signal>>();
        }

        public List<double> Run(Dictionary<DateTime, Dictionary<string, BarData>> data)
        {
            BarDataDict = data;
            foreach (var pair in data)
            {
                var signals = OnBarDatas(pair.Key, pair.Value);
                SignalDict.Add(pair.Key, signals);
            }
            return Caculate();
        }

        public 

        private List<double> Caculate()
        {
            throw new NotImplementedException();
        }
    }
}
