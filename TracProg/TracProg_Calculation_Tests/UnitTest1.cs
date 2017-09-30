using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TracProg.Calculation;

namespace TracProg_Calculation_Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            IElement el = new Pin(-10, 10, 3, 4);

            if (el is Pin)
            {
 
            }
        }
    }
}
