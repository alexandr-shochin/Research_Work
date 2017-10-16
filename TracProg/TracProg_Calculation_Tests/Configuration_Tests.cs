using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TracProg.Calculation;

namespace TracProg_Calculation_Tests
{
    [TestClass]
    public class Configuration_Tests
    {
        [TestMethod]
        public void TestMethod_Init()
        {
            Configuration config = new Configuration(@"D:\Program Files\Dropbox\Research_Work\TracProg\config.mydeflef");


        }
    }
}
