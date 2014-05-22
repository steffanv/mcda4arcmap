using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCDA.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCDA.Test
{
    [TestClass]
    public class SmallestDifferenceTest
    {

        [TestMethod]
        public void TestMethod1()
        {
            double sD = Util.SmallestDifference(new double[0]);

            Assert.AreEqual(sD, 0);

            double sD2 = Util.SmallestDifference(new [] {4d});

            Assert.AreEqual(sD2, 0);

            double sD3 = Util.SmallestDifference(new [] { 4d, -3f });

            Assert.AreEqual(sD3, 7);
        }
        [TestMethod]
        public void TestMethod2()
        {
            double sD = Util.SmallestDifference(new [] {0.3f, 0.1f, 1.4f, 8.1d, 2.0f});

            Assert.AreEqual(sD, 0.2, 0.0001);

            double sD2 = Util.SmallestDifference(new [] { 0.3d, 0.1f, 1.4f});

            Assert.AreEqual(sD2, 0.2, 0.0001);

            double sD3 = Util.SmallestDifference(new [] { -0.3f, 0.1d, -1.4f, -8.1f, 2.0f });

            Assert.AreEqual(sD3, 0.4, 0.0001);
        }
    }
}
