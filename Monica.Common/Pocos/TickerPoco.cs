using Monica.Common.Utils;
using Platinum.Common.Pocos;
using Platinum.Common.Utils;

namespace Monica.Common.Pocos
{
   public abstract  class TickerPoco:SerializablePoco
    {
        public double Margin => string.IsNullOrEmpty(Ticker) ? 0 : TickerHelper.GetGeneralTickerInfoByTicker(Ticker).Margin;

        public string Currency => string.IsNullOrEmpty(Ticker) ? string.Empty : TickerHelper.GetGeneralTickerInfoByTicker(Ticker).Currency;

        public string Exchange => string.IsNullOrEmpty(Ticker) ? string.Empty : Ticker.Split('.')[1];

        public string Code => string.IsNullOrEmpty(Ticker) ? string.Empty : Ticker.Split('.')[0];

        public string Prefix => string.IsNullOrEmpty(Ticker) ? string.Empty : TickerHelper.GetPrefixByTicker(Ticker);

        public double PointValue => string.IsNullOrEmpty(Ticker) ? 0 : TickerHelper.GetGeneralTickerInfoByTicker(Ticker).PointValue;

        public TradingSession TradingSession => string.IsNullOrEmpty(Ticker) ? null : TickerHelper.GetDaySessionByTicker(Ticker);

        public string Ticker { get; set; }
    }
}
