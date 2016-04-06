using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Monica.Common.Pocos;
using Monica.Common.Utils;

namespace Monica.Quant
{

    public delegate void SingalHandler(Signal signal);

    public delegate void SubscrptionHander(string ticker);
   
    public abstract class Strategy:LogPoco
    {
        public string StrategyId;

        public event SingalHandler OnSignal;

        public event SubscrptionHander OnSubscribeTickData;

        public StrategyConfig StrategyConfig;

        public ConcurrentDictionary<string,Signal> CurrentSignals;

        public DateTime Time;

        protected Strategy(StrategyConfig config)
        {
            StrategyConfig = config;
            StrategyId = ServiceHelper.GenerateUid();
            CurrentSignals = new ConcurrentDictionary<string, Signal>();
        }

        public void Init(DateTime time)
        {
            Time = time;
        }

        public void ProcessBarDatas(DateTime time, Dictionary<string, BarData> barDataDict)
        {
            Time = time;
            var datas = barDataDict.Where(p=> StrategyConfig.ProductInfos.Contains(TickerHelper.GetProductInfoByTicker(p.Key))).ToDictionary(p=>p.Key,p=>p.Value);
            OnBarDatas(time, datas);
        }

        protected abstract void OnBarDatas(DateTime time, Dictionary<string, BarData> barDataDict);


        public virtual void OnTickDatas(DateTime time, TickData tick)
        {
            Time = time;
        }

        protected void CommitSignal(Signal signal)
        {
            CurrentSignals.AddOrUpdate(signal.Ticker, s => signal, (s, signal1) => signal);
            OnSignal?.Invoke(signal);
        }

        protected Signal GetCurrentSignal(string ticker)
        {
            if (CurrentSignals.ContainsKey(ticker))
                return CurrentSignals[ticker];
            return new Signal(StrategyId,Time,ticker);
        }

        protected void SubscribeTickData(string ticker)
        {
            OnSubscribeTickData?.Invoke(ticker);
        }

        public abstract string GetStrategyName();
    }
}
