using System;
using System.Collections.Generic;
using System.Linq;
using Monica.Common.Utils;
using Platinum.Common.Utils;

namespace Monica.Common.Pocos
{
    public enum Periodicity
    {
        OneSecond = 1,
        TenSeconds = 10,
        OneMinute = 60,
        FiveMinutes = 300,
        FifteenMinutes = 900,
        OneHour = 3600,
        EndOfDay = 86400
    }

    public enum BarDataVersion
    {
        All,
        WithoutSettle,
        WithoutOpenInterest,
        Clean,
        Any
    }
    public class BarData:TickerPoco
    {
        private DateTime _time;
        public DateTime Time
        {
            get
            {
                return _time.Kind == DateTimeKind.Utc ? _time.ToLocalTime() : _time;
            }
            set
            {
                _time = value;
            }
        }
        public double Open { get; set; }
        public double High { get; set; }
        public double Low { get; set; }
        public double Close { get; set; }
        public double Volume { get; set; }
        public double OpenInterest { get; set; }
        public double Settlement { get; set; }


        public string ToCsv(BarDataVersion version)
        {
            switch (version)
            {
                case BarDataVersion.All:
                    return
                        $"{Time.ToString(GeneralConstants.TimeFormat)},{Open.ToString(GeneralConstants.DoubleFormat)},{High.ToString(GeneralConstants.DoubleFormat)},{Low.ToString(GeneralConstants.DoubleFormat)},{Close.ToString(GeneralConstants.DoubleFormat)},{Volume.ToString(GeneralConstants.DoubleFormat)},{Settlement.ToString(GeneralConstants.DoubleFormat)},{OpenInterest.ToString(GeneralConstants.DoubleFormat)}";
                case BarDataVersion.Clean:
                    return
                        $"{Time.ToString(GeneralConstants.TimeFormat)},{Open.ToString(GeneralConstants.DoubleFormat)},{High.ToString(GeneralConstants.DoubleFormat)},{Low.ToString(GeneralConstants.DoubleFormat)},{Close.ToString(GeneralConstants.DoubleFormat)},{Volume.ToString(GeneralConstants.DoubleFormat)}";
                case BarDataVersion.WithoutSettle:
                    return
                        $"{Time.ToString(GeneralConstants.TimeFormat)},{Open.ToString(GeneralConstants.DoubleFormat)},{High.ToString(GeneralConstants.DoubleFormat)},{Low.ToString(GeneralConstants.DoubleFormat)},{Close.ToString(GeneralConstants.DoubleFormat)},{Volume.ToString(GeneralConstants.DoubleFormat)},{OpenInterest.ToString(GeneralConstants.DoubleFormat)}";
                case BarDataVersion.WithoutOpenInterest:
                    return
                        $"{Time.ToString(GeneralConstants.TimeFormat)},{Open.ToString(GeneralConstants.DoubleFormat)},{High.ToString(GeneralConstants.DoubleFormat)},{Low.ToString(GeneralConstants.DoubleFormat)},{Close.ToString(GeneralConstants.DoubleFormat)},{Volume.ToString(GeneralConstants.DoubleFormat)},{Settlement.ToString(GeneralConstants.DoubleFormat)}";
                default:
                    throw new Exception($"Unsupport version,version={version}");
            }
        }

        public static BarData ParseFromCsv(string line, DateTime date, string ticker, BarDataVersion version)
        {
            return ParseFromCsv(line, date.ToString(GeneralConstants.DateFormat), ticker, version);
        }

        public static BarData ParseFromCsv(string line, string date, string ticker, BarDataVersion version)
        {
            var datas = line.Split(',');
            switch (version)
            {
                case BarDataVersion.Any:
                    if (datas.Length == 8)
                        return ParseFromCsv(line, date, ticker, BarDataVersion.All);
                    if (datas.Length == 6)
                        return ParseFromCsv(line, date, ticker, BarDataVersion.Clean);
                    if (datas.Length == 7)
                        return ParseFromCsv(line, date, ticker, BarDataVersion.WithoutOpenInterest);
                    if(datas.Length > 8)
                        return ParseFromCsv(line, date, ticker, BarDataVersion.Clean);
                    throw new Exception($"Invalid data length,line={line},date={date},ticker={ticker}");

                case BarDataVersion.All:
                    return new BarData
                    {
                        Ticker = ticker,
                        Time = DateTime.ParseExact(date + " " + datas[0], "yyyyMMdd H:m:ss", null),
                        Open = Convert.ToDouble(datas[1]),
                        High = Convert.ToDouble(datas[2]),
                        Low = Convert.ToDouble(datas[3]),
                        Close = Convert.ToDouble(datas[4]),
                        Volume = Convert.ToDouble(datas[5]),
                        Settlement = Convert.ToDouble(datas[6]),
                        OpenInterest = Convert.ToDouble(datas[7])
                    };
                case BarDataVersion.Clean:
                    return new BarData
                    {
                        Ticker = ticker,
                        Time = DateTime.ParseExact(date + " " + datas[0], "yyyyMMdd H:m:ss", null),
                        Open = Convert.ToDouble(datas[1]),
                        High = Convert.ToDouble(datas[2]),
                        Low = Convert.ToDouble(datas[3]),
                        Close = Convert.ToDouble(datas[4]),
                        Volume = Convert.ToDouble(datas[5])
                    };
                case BarDataVersion.WithoutSettle:
                    return new BarData
                    {
                        Ticker = ticker,
                        Time = DateTime.ParseExact(date + " " + datas[0], "yyyyMMdd H:m:ss", null),
                        Open = Convert.ToDouble(datas[1]),
                        High = Convert.ToDouble(datas[2]),
                        Low = Convert.ToDouble(datas[3]),
                        Close = Convert.ToDouble(datas[4]),
                        Volume = Convert.ToDouble(datas[5]),
                        OpenInterest = Convert.ToDouble(datas[6]),
                    };
                case BarDataVersion.WithoutOpenInterest:
                    return new BarData
                    {
                        Ticker = ticker,
                        Time = DateTime.ParseExact(date + " " + datas[0], "yyyyMMdd H:m:ss", null),
                        Open = Convert.ToDouble(datas[1]),
                        High = Convert.ToDouble(datas[2]),
                        Low = Convert.ToDouble(datas[3]),
                        Close = Convert.ToDouble(datas[4]),
                        Volume = Convert.ToDouble(datas[5]),
                        Settlement = Convert.ToDouble(datas[6])
                    };
                default:
                    throw new Exception($"Not supported version={version}");
            }
        }

        public void Validate()
        {
            if(Volume < 0)
                throw new Exception($"Volume can't be negative, barData={this}");
            if(Open > High || Open < Low || Close > High || Close < Low || Low > High)
                throw new Exception($"Price is invalid, barData={this}");
        }

        public static BarData GetDailyBarData(string ticker,DateTime date, IEnumerable<TickData> tickDatas)
        {
            var enumerable = tickDatas.OrderBy(t=>t.Time).ToArray();
            var settleTick = enumerable.LastOrDefault(t => t.SettlementPrice > double.Epsilon);
            var tradingSession = TickerHelper.GetDaySessionByTicker(ticker,date.ToString(GeneralConstants.DateFormat));
            var validTickDatas = enumerable.Where(t => tradingSession.IsInTimeSession(t.Time)).ToArray();
            if (validTickDatas.Any() == false)
                return DummyBar(date, ticker);
            return new BarData
            {
                Ticker = validTickDatas.First().Ticker,
                Time = date,
                Open = validTickDatas.First().Price,
                Close = validTickDatas.Last().Price,
                High = validTickDatas.Max(tick => tick.Price),
                Low = validTickDatas.Min(tick => tick.Price),
                Volume = validTickDatas.Sum(tick => tick.Volume),
                Settlement = settleTick != null?settleTick.SettlementPrice: validTickDatas.Last().Price,
                OpenInterest = validTickDatas.Last().OpenInterest
            };
        }

        public static BarData GetDailyBarData(string ticker, DateTime date, IEnumerable<BarData> barDatas)
        {
            var enumerable = barDatas.OrderBy(t => t.Time).ToArray();
            var tradingSession = TickerHelper.GetDaySessionByTicker(ticker, date.ToString(GeneralConstants.DateFormat));
            var validBarDatas = enumerable.Where(t => tradingSession.IsInTimeSession(t.Time)).ToArray();
            if (validBarDatas.Any() == false)
                return DummyBar(date, ticker);
            return new BarData
            {
                Ticker = validBarDatas.First().Ticker,
                Time = date,
                Open = validBarDatas.First().Open,
                Close = validBarDatas.Last().Close,
                High = validBarDatas.Max(b => b.High),
                Low = validBarDatas.Min(b => b.Low),
                Volume = validBarDatas.Sum(tick => tick.Volume),
                Settlement = validBarDatas.Last().Close,
                OpenInterest = validBarDatas.Last().OpenInterest
            };
        }

        public static BarData[] GetBarDatas(IEnumerable<TickData> tickDatas, Periodicity periodicity)
        {
            return
                tickDatas.GroupBy(tick => GetForwrdBarDataTime(tick.Time, periodicity))
                    .Select(g =>
                    {
                        return new BarData
                        {
                            Ticker = g.First().Ticker,
                            Time = g.Key,
                            Open = g.First().Price,
                            Close = g.Last().Price,
                            High = g.Max(tick => tick.Price),
                            Low = g.Min(tick => tick.Price),
                            Volume = g.Sum(tick => tick.Volume),
                            Settlement = g.Last().Price,
                            OpenInterest = g.Last().OpenInterest,
                        };
                    })
                    .ToArray();
        }

        private static DateTime GetForwrdBarDataTime(DateTime time, Periodicity periodicity)
        {
            if (time.Ticks%((int) periodicity*TimeSpan.TicksPerSecond) > 0)
                return
                    time.AddTicks((int) periodicity*TimeSpan.TicksPerSecond -
                                  time.Ticks%((int) periodicity*TimeSpan.TicksPerSecond));
            return time;
        }

        public static BarData DummyBar(DateTime time,string ticker)
        {
            return new BarData()
            {
                Ticker = ticker,
                Time = time,
                Open = double.NaN,
                High = double.NaN,
                Low = double.NaN,
                Close = double.NaN,
                Settlement = double.NaN,
                Volume = 0,
                OpenInterest = 0
            };
        }

        public override string Serialize()
        {
            return $"{Ticker},{Time.ToString(GeneralConstants.DateTimeFormat)},{Open.ToString(GeneralConstants.DoubleFormat)},{High.ToString(GeneralConstants.DoubleFormat)},{Low.ToString(GeneralConstants.DoubleFormat)},{Close.ToString(GeneralConstants.DoubleFormat)},{Volume.ToString(GeneralConstants.DoubleFormat)},{Settlement.ToString(GeneralConstants.DoubleFormat)},{OpenInterest.ToString(GeneralConstants.DoubleFormat)}";
        }

        public static BarData Deserialize(string line)
        {
            var datas = line.Split(',');
            return new BarData
            {
                Ticker = datas[0],
                Time = DateTimeHelper.Parse(datas[1]),
                Open = Convert.ToDouble(datas[2]),
                High = Convert.ToDouble(datas[3]),
                Low = Convert.ToDouble(datas[4]),
                Close = Convert.ToDouble(datas[5]),
                Volume = Convert.ToDouble(datas[6]),
                Settlement = Convert.ToDouble(datas[7]),
                OpenInterest = Convert.ToDouble(datas[8])
            };
        }

        public bool IsValid()
        {
            if (Open > 0 && High > 0 && Close > 0 && Low > 0 && double.IsNaN(Volume) == false)
                return true;
            return false;
        }
    }
}
