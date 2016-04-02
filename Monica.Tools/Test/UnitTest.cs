using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Monica.Tools.Test
{
    [TestClass()]
    public class UnitTest
    {
        [TestMethod]
        public void TestGeneateBarDatas()
        {
            Program.GenerateBarDatas(@"Test\Datas\TickData", @"Test\TestResults\BarData\60", 60);
        }

        [TestMethod]
        public void TestGenerateBackAdjustBarDatas()
        {
            Program.GenerateBackAdjustDatas(@"Test\Datas\BarData\60", @"Test\TestResults\BackAdjustData\60");
        }

    }
}
