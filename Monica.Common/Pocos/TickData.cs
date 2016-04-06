using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using Monica.Common.Utils;

namespace Monica.Common.Pocos
{

    public delegate void TickDataCallbackHandler(TickData tickData);
    [Serializable]
    public class TickData: TickerPoco
    {
        private static ILog _logger;


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
        private DateTime _localTime;
        public DateTime LocalTime
        {
            get
            {
                return _localTime.Kind == DateTimeKind.Utc ? _localTime.ToLocalTime() : _localTime;
            }
            set
            {
                _localTime = value;
            }
        }
        

        public double Price { get; set; }

        public double Volume { get; set; }

        public double TotalVolume { get; set; }
        public double TotalTurnover { get; set; }
        public double[] Bid { get; set; }
        public double[] BidVolume { get; set; }
        public double[] Ask { get; set; }
        public double[] AskVolume { get; set; }

        public double UpperLimitPrice { get; set; }

        public double LowerLimitPrice { get; set; }

        public double SettlementPrice { get; set; }

        public double OpenInterest { get; set; }

        public double Open { get; set; }

        public double High { get; set; }

        public double Low { get; set; }

        public double Close { get; set; }
        public TickData()
        {
            Volume = 0;
            Bid = new double[5];
            BidVolume = new double[5];
            Ask = new double[5];
            AskVolume = new double[5];
            LocalTime = DateTime.Now;
        }

        public override string ToString()
        {
            return $"{Time.ToString("HH:mm:ss.FFF")},{Price},{Volume}";
        }

        public string ToCsv()
        {
            var result = new List<string>
            {
                Time.ToString(GeneralConstants.TimeFormat),
                Price.ToString(GeneralConstants.DoubleFormat),
                Volume.ToString(GeneralConstants.DoubleFormat),
                TotalVolume.ToString(GeneralConstants.DoubleFormat),
                TotalTurnover.ToString(GeneralConstants.DoubleFormat),
                string.Join(",", Bid.Select(b=>b.ToString(GeneralConstants.DoubleFormat))),
                string.Join(",", BidVolume),
                string.Join(",", Ask.Select(b=>b.ToString(GeneralConstants.DoubleFormat))),
                string.Join(",", AskVolume),
                UpperLimitPrice.ToString(GeneralConstants.DoubleFormat),
                LowerLimitPrice.ToString(GeneralConstants.DoubleFormat),
                LocalTime.ToString(GeneralConstants.TimeFormat),
                OpenInterest.ToString(GeneralConstants.DoubleFormat),
                Open.ToString(GeneralConstants.DoubleFormat),
                High.ToString(GeneralConstants.DoubleFormat),
                Low.ToString(GeneralConstants.DoubleFormat),
                Close.ToString(GeneralConstants.DoubleFormat),
                SettlementPrice.ToString(GeneralConstants.DoubleFormat)
            };
            return string.Join(",", result.ToArray());
        }

        public static TickData ParseFromCsv(string line, DateTime date, string ticker)
        {
            return ParseFromCsv(line, date.ToString(GeneralConstants.DateFormat), ticker);
        }

        public static TickData ParseFromCsv(string line, string date, string ticker)
        {
                var datas = line.Split(',');
                if (datas.Length < 34)
                {
                    if (_logger == null)
                        _logger = LogManager.GetLogger("TickData");
                    _logger.Warn($"Invalid TickData, ticker={ticker},date={date}");
                    _logger.Warn($"data={line}");
                    return DummyTickData(DateTimeHelper.ParseDate(date), ticker);
                }
                var dateTimeString = date + datas[0];
                var localTimeString = date + datas[27];
                return new TickData
                {
                    Ticker = ticker,
                    Time = DateTime.ParseExact(dateTimeString, "yyyyMMddHH:mm:ss.FFF", CultureInfo.CurrentCulture),
                    Price = Convert.ToDouble(datas[1]),
                    Volume = Convert.ToDouble(datas[2]),
                    TotalVolume = Convert.ToDouble(datas[3]),
                    TotalTurnover = Convert.ToDouble(datas[4]),
                    Bid =
                        new[]
                        {
                            Convert.ToDouble(datas[5]), Convert.ToDouble(datas[6]), Convert.ToDouble(datas[7]),
                            Convert.ToDouble(datas[8]), Convert.ToDouble(datas[9])
                        },
                    BidVolume =
                        new[]
                        {
                            Convert.ToDouble(datas[10]), Convert.ToDouble(datas[11]), Convert.ToDouble(datas[12]),
                            Convert.ToDouble(datas[13]), Convert.ToDouble(datas[14])
                        },
                    Ask =
                        new[]
                        {
                            Convert.ToDouble(datas[15]), Convert.ToDouble(datas[16]), Convert.ToDouble(datas[17]),
                            Convert.ToDouble(datas[18]), Convert.ToDouble(datas[19])
                        },
                    AskVolume =
                        new[]
                        {
                            Convert.ToDouble(datas[20]), Convert.ToDouble(datas[21]), Convert.ToDouble(datas[22]),
                            Convert.ToDouble(datas[23]), Convert.ToDouble(datas[24])
                        },
                    UpperLimitPrice = Convert.ToDouble(datas[25]),
                    LowerLimitPrice = Convert.ToDouble(datas[26]),
                    LocalTime = DateTime.ParseExact(localTimeString, "yyyyMMddHH:mm:ss.FFF", CultureInfo.CurrentCulture),
                    OpenInterest = Convert.ToDouble(datas[28]),
                    Open = Convert.ToDouble(datas[29]),
                    High = Convert.ToDouble(datas[30]),
                    Low = Convert.ToDouble(datas[31]),
                    Close = Convert.ToDouble(datas[32]),
                    SettlementPrice = Convert.ToDouble(datas[33]),
                };
            
        }

        public bool Validate()
        {
            if (DateTimeHelper.Abs(Time, LocalTime) > TimeSpan.FromHours(0.5))
                return false;
            if (IsValidPrice(Price) == false)
                return false;
            if (IsValidVolume(Volume) == false)
                return false;
            return true;
        }

        public void Adjust()
        {
            if (IsValidVolume(TotalVolume) == false)
                TotalVolume = 0;
            if (IsValidVolume(TotalTurnover) == false)
                TotalTurnover = 0;
            if (IsValidPrice(UpperLimitPrice) == false)
                UpperLimitPrice = 0;
            if (IsValidPrice(LowerLimitPrice) == false)
                LowerLimitPrice = 0;
            if (IsValidPrice(Open) == false)
                Open = 0;
            if (IsValidPrice(High) == false)
                High = 0;
            if (IsValidPrice(Low) == false)
                Low = 0;
            if (IsValidPrice(Close) == false)
                Close = 0;
            if (IsValidPrice(SettlementPrice) == false)
                SettlementPrice = 0;
            if (IsValidVolume(OpenInterest) == false)
                OpenInterest = 0;
            for (var i = 0; i < 5; i++)
            {
                if (IsValidPrice(Bid[i]) == false)
                    Bid[i] = 0;
                if (IsValidVolume(BidVolume[i]) == false)
                    BidVolume[i] = 0;
                if (IsValidPrice(Ask[i]) == false)
                    Ask[i] = 0;
                if (IsValidVolume(AskVolume[i]) == false)
                    AskVolume[i] = 0;
            }
        }

        public static bool IsValidPrice(double value)
        {
            return value > 0 && value < 9999999;
        }

        public static bool IsValidVolume(double value)
        {
            return value >= 0 && value < 9999999999;
        }

        public static TickData DummyTickData(DateTime time, string ticker)
        {
            return new TickData()
            {
                Ticker = ticker,
                Time = time,
                Price = double.NaN,
                Volume = 0,
                Bid = new []{double.NaN, double.NaN , double.NaN , double.NaN , double.NaN },
                Ask = new[] { double.NaN , double.NaN , double.NaN , double.NaN, double.NaN },
                SettlementPrice = double.NaN
                
            };
        }

        public override string Serialize()
        {
            var result = new List<string>
            {
                Time.ToString("yyyy-MM-dd HH:mm:ss.FFF"),
                Price.ToString("0.####"),
                Volume.ToString("0.####"),
                TotalVolume.ToString("0.####"),
                TotalTurnover.ToString("0.####"),
                string.Join(",", Bid),
                string.Join(",", BidVolume),
                string.Join(",", Ask),
                string.Join(",", AskVolume),
                UpperLimitPrice.ToString(CultureInfo.InvariantCulture),
                LowerLimitPrice.ToString(CultureInfo.InvariantCulture),
                SettlementPrice.ToString("0.####"),
                OpenInterest.ToString("0.####"),
                Ticker,
            };
            return string.Join(",", result.ToArray());
        }

        public static TickData Deserialize(string line)
        {
            var datas = line.Split(',');
            return new TickData
            {
                Time = DateTimeHelper.ParseDateTime(datas[0]),
                Price = Convert.ToDouble(datas[1]),
                Volume = Convert.ToDouble(datas[2]),
                TotalVolume = Convert.ToDouble(datas[3]),
                TotalTurnover = Convert.ToDouble(datas[4]),
                Bid =
                    new[]
                    {
                            Convert.ToDouble(datas[5]), Convert.ToDouble(datas[6]), Convert.ToDouble(datas[7]),
                            Convert.ToDouble(datas[8]), Convert.ToDouble(datas[9])
                    },
                BidVolume =
                    new[]
                    {
                            Convert.ToDouble(datas[10]), Convert.ToDouble(datas[11]), Convert.ToDouble(datas[12]),
                            Convert.ToDouble(datas[13]), Convert.ToDouble(datas[14])
                    },
                Ask =
                    new[]
                    {
                            Convert.ToDouble(datas[15]), Convert.ToDouble(datas[16]), Convert.ToDouble(datas[17]),
                            Convert.ToDouble(datas[18]), Convert.ToDouble(datas[19])
                    },
                AskVolume =
                    new[]
                    {
                            Convert.ToDouble(datas[20]), Convert.ToDouble(datas[21]), Convert.ToDouble(datas[22]),
                            Convert.ToDouble(datas[23]), Convert.ToDouble(datas[24])
                    },
                UpperLimitPrice = Convert.ToDouble(datas[25]),
                LowerLimitPrice = Convert.ToDouble(datas[26]),
                SettlementPrice = Convert.ToDouble(datas[27]),
                OpenInterest = Convert.ToDouble(datas[28]),
                Ticker = datas[29]
            };
        }
    }
}
