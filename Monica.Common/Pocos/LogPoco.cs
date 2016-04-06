using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using log4net.Repository.Hierarchy;

namespace Monica.Common.Pocos
{
    public abstract class LogPoco 
    {
        protected ILog Logger;

        protected LogPoco()
        {
            Logger = LogManager.GetLogger(GetType().ToString());
        }
    }
}
