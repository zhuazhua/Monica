using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Monica.Quant.Simulators;
using Monica.Quant.Strategies;

namespace Monica.Quant
{
    public class Program
    {
        private static readonly ILog Logger = LogManager.GetLogger("Monica.Quant");

        public static void Main(string[] args)
        {
            try
            {
                var fileSimulatorConfig = new FileSimulatorConfig()
                {
                    Start = new DateTime(2016, 01, 04),
                    End = new DateTime(2016, 02, 01),
                    BarDataPath = @"..\BarData",
                    BarSize = 60,
                    ProductInfos = new List<string>() {"IF.CFFEX"},
                    Type = SimulatorType.File,
                    SimulationResultPath = @"..\Simulation"
                };
                var fileSimulator = new FileSimulator(fileSimulatorConfig);
                var strategyConfig = new StrategyConfig()
                {
                    Name = "TestStrategy",
                    StrategyType = StrategyType.Intraday,
                };
                var strategy = new TestStrategy(strategyConfig); 
                fileSimulator.Init();
                fileSimulator.Run(strategy);
                fileSimulator.DumpDetails(strategy.StrategyId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message,ex);
            }
            Console.ReadKey();
        }
    }
}
