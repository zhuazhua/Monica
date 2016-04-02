using System;

namespace Monica.Common.Pocos
{
    [Serializable]
    public class GeneralTickerInfo
    {
        public string InternalProductInfo => Prefix + "," + InternalProduct + "." + Exchange;

        public string ProductInfo => Product + "." + Exchange;
        public String Adapter { get; set; }
        public String InternalProduct { get; set; }
        public string Prefix { get; set; }
        public String Exchange { get; set; }
        public int ExchangeTimezoneIndex { get; set; }
        public String Currency { get; set; }
        public double PointValue { get; set; }
        public double MinMove { get; set; }
        public double LotSize { get; set; }
        public double ExchangeRateXxxUsd { get; set; }
        public int LocalTimezoneIndex { get; set; }
        public double CommissionOnRate { get; set; }
        public double CommissionPerShareInXxx { get; set; }
        public double MinCommissionInXxx { get; set; }
        public double MaxCommissionInXxx { get; set; }
        public double StampDutyRate { get; set; }
        public double Slippage { get; set; }
        public string Product { get; set; }
        public bool IsCloseTodayFree { get; set; }

        public bool IsLive { get; set; }

        public double Margin { get; set; }

        public static GeneralTickerInfo ParseFromCsv(string line, Char seperator = ',')
        {
            var data = line.Split(seperator);
            return new GeneralTickerInfo
            {
                Adapter = data[0],
                InternalProduct = data[1],
                Exchange = data[2],
                Prefix = data[3],
                LocalTimezoneIndex = int.Parse(data[4]),
                ExchangeTimezoneIndex = int.Parse(data[5]),
                Currency = data[6],
                PointValue = double.Parse(data[7]),
                MinMove = double.Parse(data[8]),
                LotSize = double.Parse(data[9]),
                ExchangeRateXxxUsd = double.Parse(data[10]),
                CommissionOnRate = double.Parse(data[13]),
                CommissionPerShareInXxx = double.Parse(data[14]),
                MinCommissionInXxx = double.Parse(data[15]),
                MaxCommissionInXxx = double.Parse(data[16]),
                StampDutyRate = double.Parse(data[17]),
                Slippage = double.Parse(data[18]),
                Product = data[19],
                IsCloseTodayFree = bool.Parse(data[20]),
                Margin = double.Parse(data[21])/100,
                IsLive = bool.Parse(data[22])
            };
        }
    }
}
