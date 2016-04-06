using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using log4net;
using Monica.Common.Pocos;

namespace Monica.Common.Utils
{
    public class BackAdjustHelper
    {
        private static readonly ILog Logger = LogManager.GetLogger("BackAdjustHelper");

        public static void GenerateMostActiveContracts(string inDir, string outPath, Func<string, FutureContractInfo> getFutureContractInfo)
        {
            var inDirInfo = new DirectoryInfo(inDir);
            var generalTickerInfos = TickerHelper.GetGeneralTickerInfos();
            var results = new List<string>();
            foreach (var tickerInfo in generalTickerInfos)
            {
                Logger.Info($"Process {tickerInfo.ProductInfo}");
                var lastMajorTicker = "";
                var lastMajorTickerPrice = 0.0;
                foreach (var dailyDir in inDirInfo.EnumerateDirectories().OrderBy(d=>d.Name))
                {
                    var date = dailyDir.Name;
                    if (DateTimeHelper.IsTradingDay(date, "yyyyMMdd", tickerInfo.Exchange) == false)
                        continue;
                    var daySession = TickerHelper.GetDaySessionByProductInfo(tickerInfo.ProductInfo, date);
                    var barDataFiles =
                        dailyDir.EnumerateFiles("*.csv")
                            .Where(f => TickerHelper.GetProductInfoByFilename(f.Name) == tickerInfo.ProductInfo).ToArray();
                    var barDataDict = barDataFiles.ToDictionary(f => TickerHelper.GetTickerByFilename(f.Name),
                        f =>
                            File.ReadAllLines(f.FullName)
                                .Select(
                                    l =>
                                        BarData.ParseFromCsv(l, date, TickerHelper.GetTickerByFilename(f.Name),
                                            BarDataVersion.Clean)).Where(b=>daySession.IsInTimeSession(b.Time)));
                    var majorTicker =
                        barDataDict.OrderByDescending(d => d.Value.Sum(b => b.Volume)*d.Value.Count())
                            .Select(o => o.Key)
                            .FirstOrDefault();
                    if (majorTicker == null || barDataDict[majorTicker].Any() == false)
                        continue;
                    var futureContractInfo = getFutureContractInfo(majorTicker);
                    if (futureContractInfo != null)
                    {
                        if (DateTimeHelper.Parse(date) >= DateTimeHelper.GetLastTradingDay(DateTimeHelper.Parse(futureContractInfo.ExpireDate)))
                        {
                           var  secondMajorTicker = barDataDict.OrderByDescending(d => d.Value.Sum(b => b.Volume) * d.Value.Count())
                            .Select(o => o.Key).Skip(1)
                            .FirstOrDefault();
                            if (string.IsNullOrEmpty(secondMajorTicker) == false)
                                majorTicker = secondMajorTicker;
                        }
                    }
                    if(majorTicker == null || barDataDict[majorTicker].Any() == false)
                        continue;
                    if (string.IsNullOrEmpty(lastMajorTicker))
                    {
                        results.Add($"{date},{tickerInfo.ProductInfo},{majorTicker},0");
                    }
                    else if (majorTicker != lastMajorTicker)
                    {
                        if (barDataDict.ContainsKey(lastMajorTicker) && barDataDict[lastMajorTicker].Any())
                        {
                            var open1 = barDataDict[majorTicker].First().Open;
                            var open2 = barDataDict[lastMajorTicker].First().Open;
                            var adjust = open1 - open2;
                            results.Add($"{date},{tickerInfo.ProductInfo},{majorTicker},{adjust}");
                        }
                        else
                        {
                            var adjust = barDataDict[majorTicker].First().Open - lastMajorTickerPrice;
                            results.Add($"{date},{tickerInfo.ProductInfo},{majorTicker},{adjust}");
                        }
                    }
                    lastMajorTicker = majorTicker;
                    lastMajorTickerPrice = barDataDict[majorTicker].Last().Close;
                }
            }
            File.WriteAllLines(outPath,results);
        }

    }
}
