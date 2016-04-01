using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Monica.Quant.Simulator
{
  public  class SimulatorConfig
    {
        public DateTime Start { get; set; }

        public DateTime End { get; set; }

        public List<string> ProductInfos { get; set; } 

    }
}
