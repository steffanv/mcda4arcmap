using System;
using System.Collections.ObjectModel;
using ESRI.ArcGIS.Carto;
using MCDA.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using Moq;

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
            //var mcdaExtension = MCDAExtension.GetExtension();
            //var mock = new Mock<MCDAExtension>();
            //mock.Setup(extension => extension.AvailableFeatures).Returns(new ObservableCollection<Feature>());
            //MCDAExtension Extensions = MCDAExtension.GetExtension();

            //DataTable = new DataTable();

            var toolParameter = new ToolParameter("test") {IsOID = true};
            var toolParameter2 = new ToolParameter("test2") {Weight = 33.33d};
            var toolParameter3 = new ToolParameter("test3") { Weight = 33.33d };
            var toolParameter4 = new ToolParameter("test4") { Weight = 33.33d };

            //object o = mock.Object.AvailableFeatures;

            ToolParameterContainer = new Model.ToolParameterContainer(new List<IToolParameter>() { toolParameter, toolParameter2, toolParameter3, toolParameter4});

            DataTable = new DataTable();
            DataTable.Columns.Add("test", typeof(int));
            DataTable.Columns.Add("test2", typeof(int));
            DataTable.Columns.Add("test3", typeof(int));
            DataTable.Columns.Add("test4", typeof(int));

            DataTable.Rows.Add(2, 22, 4444, 1);
            DataTable.Rows.Add(3, 22, 4444, 1);
            DataTable.Rows.Add(4, 22, 4444, 1);
            DataTable.Rows.Add(5, 22, 4444, 1);
            DataTable.Rows.Add(6, 22, 4444, 1);

            //ToolFactory.NewLWLCTool();
        }

        [TestMethod]
        public void TestMethod1()
        {
            Assert.AreEqual(4, 4);
        }

        [TestMethod]
        public void TestMethod2()
        {
            var wlcTool = new MCDA.Model.WLCTool(DataTable, ToolParameterContainer);

            wlcTool.Run();

            Assert.IsNotNull(wlcTool.Data);
            Assert.IsTrue(wlcTool.Data.Rows.Count == 5);
            Assert.AreEqual(wlcTool.DefaultResultColumnName, "WLCResult");
            var item = wlcTool.Data.Rows[0].ItemArray[4];
        }
    }
}
