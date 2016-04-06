using System.Collections.Generic;
using System.IO;
using System.Linq;
using Monica.Common.Pocos;
using Monica.Common.Utils;

namespace Monica.Quant.Simulators
{
    public class FileSimulator:Simulator
    {
        private readonly FileSimulatorConfig _config;

        public FileSimulator(FileSimulatorConfig config) : base(config)
        {
            _config = config;
        }

        protected override void LoadDatas()
        {
            var inDir = new DirectoryInfo(Path.Combine(_config.BarDataPath,_config.BarSize.ToString()));
            if(inDir.Exists == false)
                throw new DirectoryNotFoundException($"Director {inDir.Name} not found");
            //Load BarDatas From File
            foreach (var file in inDir.EnumerateFiles("*.csv",SearchOption.AllDirectories))
            {
                var date = DateTimeHelper.ParseDate(file.Directory.Name);
                if (date < _config.Start || date > _config.End)
                    continue;
                var productInfo = TickerHelper.GetProductInfoByFilename(file.Name);
                if(_config.ProductInfos.Contains(productInfo) == false)
                    continue;
                var ticker = TickerHelper.GetTickerByFilename(file.Name);
                var tradingSession = TickerHelper.GetDaySessionByTicker(ticker, file.Directory.Name);
                var barDatas =
                    File.ReadAllLines(file.FullName)
                        .Select(l => BarData.ParseFromCsv(l, date, ticker, BarDataVersion.Any)).Where(b=>tradingSession.IsInTimeSession(b.Time));
                foreach (var barData in barDatas)
                {
                    var time = barData.Time.ToBinary();
                    if (BarDataDict.ContainsKey(time) == false)
                        BarDataDict.Add(time, new Dictionary<string, BarData>());
                    BarDataDict[time].Add(barData.Ticker, barData);
                    if(PriceDict.ContainsKey(barData.Ticker) == false)
                        PriceDict.Add(barData.Ticker,new Dictionary<long, BarData>());
                    PriceDict[barData.Ticker].Add(time,barData);
                }
            }
        }
    }
}
