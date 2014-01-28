using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;

namespace MCDA.Test
{
    [TestClass]
    public class ScoreRangeStandardizationTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            //DataColumn dataColumn = BuildDataTable<double>(new[] { -3d, 90d, 17d });

            MCDA.Model.IStandardizationStrategy transformationStrategy = MCDA.Model.StandardizationStrategyFactory.GetStrategy(Model.StandardizationStrategy.ScoreRangeStandardizationStrategy);

            //transformationStrategy.Transform(dataColumn);

            //double max = dataColumn.Table.AsEnumerable().Select(c => c.Field<double>(0)).Max();
            //double min = dataColumn.Table.AsEnumerable().Select(c => c.Field<double>(0)).Min();

            //Assert.AreEqual(max, 1);
            //Assert.AreEqual(min, 0);
        }
    }
}
