using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using MathNet.Numerics.Financial;
using MathNet.Numerics.Statistics;
using Monica.Common.Pocos;
using Monica.Quant.Utils;

namespace Monica.Quant.Simulators
{
    public class DailyResult
    {
        public double PnL { get; set; }

        public double Commission { get; set; }

        public double Slipage { get; set; }

        public DailyResult()
        {
            PnL = 0;
            Commission = 0;
            Slipage = 0;
        }

        public DailyResult(double pnl,double commission,double slipage)
        {
            PnL = pnl;
            Commission = commission;
            Slipage = slipage;
        }

        public static DailyResult operator +(DailyResult s1, DailyResult s2)
        {
            return new DailyResult()
            {
                PnL = s1.PnL + s2.PnL,
                Commission = s1.Commission + s2.Commission,
                Slipage = s1.Slipage + s2.Slipage
            };
        }

        public static DailyResult operator *(DailyResult s1, double multipier)
        {
            return new DailyResult()
            {
                PnL = s1.PnL*multipier,
                Commission = s1.Commission*multipier,
                Slipage = s1.Slipage*multipier,
            };
        }

        public string ToCsv()
        {
            return $"{PnL.ToString(GeneralConstants.PercentFormat)},{Commission.ToString(GeneralConstants.PercentFormat)},{Slipage.ToString(GeneralConstants.PercentFormat)}";
        }
    }


    public class SimulationResult
    {
       public Dictionary<string, DailyResult> Pnls { get; set; }

        public double Risk => Pnls.Values.Select(v => v.PnL).ToArray().StandardDeviation()*Math.Sqrt(250);

        public double Return => Pnls.Values.Select(v => v.PnL).ToArray().Mean()*250;

        public double Commission => Pnls.Values.Select(v => v.Commission).ToArray().Mean() * 250;

        public double Slipage => Pnls.Values.Select(v => v.Slipage).ToArray().Mean() * 250;

        public double Sharp => Return/Risk;

       public double[] NetValues => QuantHelper.NetValues(Pnls.Values.Select(v => v.PnL).ToArray());

       public double MaxDrawdown => QuantHelper.MaxDrawdown(NetValues);

       public string[] Dates => Pnls.Keys.ToArray();

        public double Multipier { get; set; }

       public DailyResult GetDailyResult(string date)
       {
           if (Pnls.ContainsKey(date) == false)
               return new DailyResult();
           return Pnls[date];
       }

        public SimulationResult(Dictionary<string, DailyResult> pnls)
        {
            Pnls = pnls;
        }

        public void AddDailyResult(string date, DailyResult result)
        {
            if(Pnls.ContainsKey(date))
                throw new Exception($"{date} result exists");
            Pnls.Add(date,result);
        }

        public void Standardlize()
       {
            if (!(Risk > double.Epsilon)) return;
            var multipier = 0.12/Risk;
            Multipier = 1000000*multipier;
            foreach (var key in Pnls.Keys.ToArray())
            {
                Pnls[key] *= multipier;
            }
       }

        public override string ToString()
        {

            var netValues = QuantHelper.NetValues(Pnls.Values.Select(v => v.PnL).ToArray());
            var builder = new StringBuilder();
            var datas = Pnls.Select((p, i) => $"{p.Key},{p.Value.ToCsv()},{netValues[i].ToString(GeneralConstants.DoubleFormat)}");
            builder.AppendLine("Date,Pnl,Commission,Slipage,NetValue");
            foreach (var data in datas)
            {
                builder.AppendLine(data);
            }
            builder.AppendLine($"Return (Annua.),{Return.ToString(GeneralConstants.PercentFormat)}");
            builder.AppendLine($"Commission (Annua.),{Commission.ToString(GeneralConstants.PercentFormat)}");
            builder.AppendLine($"Slipage (Annua.),{Slipage.ToString(GeneralConstants.PercentFormat)}");
            builder.AppendLine($"Risk (Annua.),{Risk.ToString(GeneralConstants.PercentFormat)}");
            builder.AppendLine($"Sharp,{Sharp.ToString(GeneralConstants.DoubleFormat)}");
            builder.AppendLine($"MaxDrawdown,{MaxDrawdown.ToString(GeneralConstants.PercentFormat)}");
            builder.AppendLine($"Multipier,{Multipier.ToString(GeneralConstants.DoubleFormat)}");
            return builder.ToString();
        }

        public void Dump(string filename)
       {
           File.WriteAllText(filename, this.ToString());
       }

       public static SimulationResult operator +(SimulationResult s1, SimulationResult s2)
       {
           var dates = s1.Pnls.Keys.Union(s2.Pnls.Keys).Distinct();
           var pnls = dates.ToDictionary(date => date, date => s1.GetDailyResult(date) + s2.GetDailyResult(date));
           return new SimulationResult(pnls);
       }

       public static SimulationResult operator *(SimulationResult s1, double mutiplier)
       {
           var pnls = s1.Pnls.ToDictionary(p => p.Key, p => p.Value*mutiplier);
           return new SimulationResult(pnls);
       }
    }
}
