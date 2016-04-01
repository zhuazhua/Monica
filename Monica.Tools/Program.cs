using System;
using CommandLine;
using log4net;

namespace Monica.Tools
{
    class Program
    {
        private static ILog _logger;
        public static int Main(string[] args)
        {
            _logger = LogManager.GetLogger("Monica.Tools");
            try
            {
                var options = new Options();
                if (Parser.Default.ParseArguments(args, options))
                {
                    switch (options.Action)
                    {
                        case "GenerateBarData":
                            GenerateBarData(options.InDir, options.OutDir, options.BarSize);
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
                _logger.Error(ex.Message, ex);
                return -1;
            }
            return 0;
        }

        private static void GenerateBarData(string inDir,string outDir,int barSize)
        {
            
        }
        
    }
}
