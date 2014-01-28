using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.IO;
using System.Collections.Generic;

namespace MCDA.Test
{
    [TestClass]
    public class MaximumScoreStandardizationTest
    {
        // -3, -2, 0
        // 0, 0, 0
        // 1, 1, 1

        [TestMethod]
        public void TestMethod1()
        {

            DataColumn dataColumn = BuildDataTable<double>(new[] { -3d, 90d, 17d });

            MCDA.Model.IStandardizationStrategy transformationStrategy = MCDA.Model.StandardizationStrategyFactory.GetStrategy(Model.StandardizationStrategy.MaximumScoreStandardizationStrategy);

            transformationStrategy.Transform(dataColumn);

            double max = dataColumn.Table.AsEnumerable().Select(c => c.Field<double>(0)).Max();
            double min = dataColumn.Table.AsEnumerable().Select(c => c.Field<double>(0)).Min();

            Assert.AreEqual(max, 1);
            Assert.AreEqual(min, 0);

        }

        private DataColumn BuildDataTable<T>(IEnumerable<T> data)
        {

            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn("Column", typeof(T));
            dt.Columns.Add(dc);

            foreach (var currentValue in data)
            {
                DataRow dr = dt.NewRow();
                dr[0] = currentValue;
                dt.Rows.Add(dr);
            }

            return dt.Columns[0];
        }
    }
}
