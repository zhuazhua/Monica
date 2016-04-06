using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monica.Quant
{
    public enum SimulatorType
    {
        File
    }

  public  class SimulatorConfig
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public List<string> ProductInfos { get; set; } 

        public SimulatorType Type { get; set; }

        public int BarSize { get; set; }

        public string SimulationResultPath { get; set; }

    }
}
