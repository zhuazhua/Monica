using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CommandLine;
using log4net;
using Monica.Common.Pocos;
using Monica.Common.Utils;
using Platinum.Common.Utils;

namespace Monica.Tools
{
    class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("Monica.Tools");
        public static int Main(string[] args)
        {
            try
            {
                var options = new Options();
                if (Parser.Default.ParseArguments(args, options))
                {
                    switch (options.Action)
                    {
                        case "GenerateBarData":
                            GenerateBarDatas(options.InDir, options.OutDir, options.BarSize);
                            break;
                        case "GenerateBackAdjustDatas":
                            GenerateBackAdjustDatas(options.InDir, options.OutDir);
                            break;
                        default:
                            Console.Error.WriteLine("Action not support.");
                            Console.Write(options.GetUsage());
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message, ex);
                return -1;
            }
            return 0;
        }

        public static void GenerateBarDatas(string inDir,string outDir,int barSize)
        {
            var inDirInfo = new DirectoryInfo(inDir);
            foreach (var file in inDirInfo.EnumerateFiles("*.csv", SearchOption.AllDirectories))
            {
                Logger.Info($"Process {file.FullName}");
                var ticker = TickerHelper.GetTickerByFilename(file.Name);
                var date = file.Directory.Name;
                var tickDatas = File.ReadAllLines(file.FullName).Select(l => TickData.ParseFromCsv(l, date, ticker)).ToArray();
                var barDatas = BarData.GetBarDatas(tickDatas, barSize);
                var outDailyDir = Path.Combine(outDir, date);
                SystemHelper.CreateDirectory(outDailyDir);
                var outFilename = Path.Combine(outDailyDir, ticker + ".csv");
                File.WriteAllLines(outFilename,barDatas.Select(b=>b.ToCsv(BarDataVersion.All)));
            }
        }

        public static void GenerateBackAdjustDatas(string inDir, string outDir)
        {
            var inDirInfo = new DirectoryInfo(inDir);
            var mostActiveTickerDict = GetMostActiveTickerDict();
            var productInfos = TickerHelper.GetGeneralTickerInfosByAdapter("CTP").Select(g => g.ProductInfo).ToArray();
            foreach (var productInfo in productInfos)
            {
                Logger.Info($"Process {productInfo}");
                var currentTicker = GetMostActiveTicker(DateTime.Today, productInfo, mostActiveTickerDict);
                foreach (var dailyDir in inDirInfo.EnumerateDirectories())
                {
                    var date = dailyDir.Name;
                    var mostActiveTicker = GetMostActiveTicker(DateTimeHelper.ParseDate(date), productInfo,
                        mostActiveTickerDict);
                    var filePath = Path.Combine(inDir, date, mostActiveTicker + ".csv");
                    if (File.Exists(filePath))
                    {
                        var backAdjustValue = GetBackAdjustValue(DateTimeHelper.ParseDate(date), productInfo,
                            mostActiveTickerDict);
                        var barDatas =
                            File.ReadAllLines(filePath)
                                .Select(l => BarData.ParseFromCsv(l, date, currentTicker, BarDataVersion.Any))
                                .ToList();
                        foreach (var barData in barDatas)
                        {
                            barData.Open += backAdjustValue;
                            barData.High += backAdjustValue;
                            barData.Low += backAdjustValue;
                            barData.Close += backAdjustValue;
                            barData.Settlement += backAdjustValue;
                        }
                        var outDailyDir = Path.Combine(outDir, date);
                        SystemHelper.CreateDirectory(outDailyDir);
                        File.WriteAllLines(Path.Combine(outDailyDir,currentTicker+".csv"),barDatas.Select(b=>b.ToCsv(BarDataVersion.All)));
                    }
                }
            }
        }

        private static Dictionary<string, Dictionary<DateTime, Tuple<string, double>>> GetMostActiveTickerDict()
        {
            var mostActiveTickerDict = new Dictionary<string, Dictionary<DateTime, Tuple<string, double>>>();
            var lines = File.ReadAllLines(@"Data\MostActiveTickers.csv");
            foreach (var line in lines)
            {
                var date = DateTimeHelper.ParseDate(line.Split(',')[0]);
                var productInfo = line.Split(',')[1];
                var ticker = line.Split(',')[2];
                var adjust = double.Parse(line.Split(',')[3]);
                if (mostActiveTickerDict.ContainsKey(productInfo) == false)
                    mostActiveTickerDict.Add(productInfo, new Dictionary<DateTime, Tuple<string, double>>());
                if (mostActiveTickerDict[productInfo].ContainsKey(date) == false)
                    mostActiveTickerDict[productInfo].Add(date, new Tuple<string, double>(ticker, adjust));
                else
                {
                    throw new Exception($"Duplicate most active ticker record, line ={line}!!!");
                }
            }

            return mostActiveTickerDict;
        }

        private static string GetMostActiveTicker(DateTime date,string productInfo,
            Dictionary<string, Dictionary<DateTime, Tuple<string, double>>> mostActiveTickerDict)
        {
            if (mostActiveTickerDict.ContainsKey(productInfo) == false)
                throw new Exception($"Uable to find most active ticker, productInfo = {productInfo},date={date}");
            var key = mostActiveTickerDict[productInfo].Keys.LastOrDefault(p => date >= p);
            if (key == null || mostActiveTickerDict[productInfo].ContainsKey(key) == false)
                throw new Exception($"Unable to find most active ticker, productInfo = {productInfo},date={date}");
            return mostActiveTickerDict[productInfo][key].Item1;
        }


        private static double GetBackAdjustValue(DateTime date, string productInfo,
            Dictionary<string, Dictionary<DateTime, Tuple<string, double>>> mostActiveTickerDict)
        {
            if (mostActiveTickerDict.ContainsKey(productInfo) == false)
                return 0;
            var adjust = mostActiveTickerDict[productInfo].Where(p => p.Key > date).Sum(p => p.Value.Item2);
            return adjust;
        }


    }
}
