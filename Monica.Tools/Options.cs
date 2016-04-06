using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CommandLine;
using CommandLine.Text;
using Monica.Common.Utils;

namespace Monica.Tools
{
    public class Options
    {
        [Option('a', "action", Required = true, HelpText = "Action to proceed.")]
        public string Action { get; set; }

        [Option('i', "inDir", HelpText = "InDir")]
        public string InDir { get; set; }

        [Option('o', "outDir", HelpText = "OutDir")]
        public string OutDir { get; set; }

        [Option('s', "startDate", HelpText = "StartDate, format-<yyyyMMdd>")]
        public string StartDateString { get; set; }

        public DateTime StartDate => string.IsNullOrEmpty(StartDateString) ? DateTime.Today : DateTimeHelper.Parse(StartDateString);

        [Option('e', "endtDate", HelpText = "EndDate, format-<yyyyMMdd>")]
        public string EndDateString { get; set; }

        [Option('b',"barSize")]
        public int BarSize { get; set; }

        public DateTime EndDate => string.IsNullOrEmpty(EndDateString) ? DateTime.Today : DateTimeHelper.Parse(EndDateString);

        [ParserState]
        public IParserState LastParserState { get; set; }

        public Options()
        {
        }

        [HelpOption]
        public string GetUsage()
        {
            var helpText = File.ReadAllText(@"Data\Help.txt");
            var builder = new StringBuilder();
            builder.Append(HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current)));
            builder.Append(helpText);
            return builder.ToString();
        }
    }
}
