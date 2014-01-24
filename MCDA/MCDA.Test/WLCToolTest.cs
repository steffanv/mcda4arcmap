using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Collections.Generic;
using System.Linq;

namespace MCDA.Test
{
    [TestClass]
    public class WLCToolTest
    {
        DataTable DataTable { get; set; }
        Model.ToolParameterContainer ToolParameterContainer { get; set; }

        [TestInitialize]
        public void Init()
        {
            DataTable = new DataTable();
            ToolParameterContainer = new Model.ToolParameterContainer(Enumerable.Empty<Model.IToolParameter>().ToList());
        }
        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(4, 4);
        }

        [TestMethod]
        public void TestMethod2()
        {
            MCDA.Model.WLCTool wlcTool = new MCDA.Model.WLCTool(DataTable, ToolParameterContainer);

            wlcTool.Run();

            Assert.IsNotNull(wlcTool.Data);
            Assert.IsTrue(wlcTool.Data.Rows.Count == 0);
            Assert.AreEqual(wlcTool.DefaultResultColumnName, "WLCResult");
        }
    }
}
