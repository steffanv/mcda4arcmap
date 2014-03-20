using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Linq;

namespace MCDA.Test
{
    [TestClass]
    public class IdentityNormalizationTest
    {
        private MCDA.Model.INormalizationStrategy IdentityNormalizationStrategy { get; set; }

        [TestInitialize]
        public void Init()
        {
            IdentityNormalizationStrategy = MCDA.Model.NormalizationStrategyFactory.GetStrategy(Model.NormalizationStrategy.IdentityNormalizationStrategy);
        }

        [TestMethod]
        public void TestMethod1()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { -3d, -2d, 0d });

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), -3);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), -2);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 0);
        }

        [TestMethod]
        public void TestMethod2()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { 0d, 0d, 0d });

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), 0);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), 0);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 0);
        }

        [TestMethod]
        public void TestMethod3()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { 1d, 1d, 1d });

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 1);
        }

        [TestMethod]
        public void TestMethod4()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { 1d, 2d, 3d });

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), 2);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 3);
        }

        [TestMethod]
        public void TestMethod5()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { -3d, -2d, 0d });

            IdentityNormalizationStrategy.Transform(dataColumn, false);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), -3);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), -2);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 0);
        }

        [TestMethod]
        public void TestMethod6()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { 0d, 0d, 0d });

            IdentityNormalizationStrategy.Transform(dataColumn, false);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), 0);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), 0);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 0);
        }

        [TestMethod]
        public void TestMethod7()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { 1d, 1d, 1d });

            IdentityNormalizationStrategy.Transform(dataColumn, false);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 1);
        }

        [TestMethod]
        public void TestMethod8()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(new[] { 1d, 2d, 3d });

            IdentityNormalizationStrategy.Transform(dataColumn, false);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<double>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<double>(0), 2);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<double>(0), 3);
        }

        [TestMethod]
        public void TestMethod9()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<int>(new[] { 1, 2, 3 });

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<int>(0), 1);
            Assert.AreEqual(dataColumn.Table.Rows[1].Field<int>(0), 2);
            Assert.AreEqual(dataColumn.Table.Rows[2].Field<int>(0), 3);
        }

        [TestMethod]
        public void TestMethod10()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<string>(new[] { "string" });

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows[0].Field<string>(0), "string");
        }

        [TestMethod]
        public void TestMethod11()
        {
            DataColumn dataColumn = TestUtil.CreateDataColumn<double>(Enumerable.Empty<double>());

            IdentityNormalizationStrategy.Transform(dataColumn);

            Assert.AreEqual(dataColumn.Table.Rows.Count, 0);
        }
    }
}
