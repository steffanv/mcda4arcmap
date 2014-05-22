using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCDA.Extensions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCDA.Test
{
    [TestClass]
    public class ObserveableCollectionTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            var observableCollection = new ObservableCollection<string> {"bCa", "abc", "caB"};

            observableCollection.SortBy(i => i);

            Assert.AreEqual(observableCollection[0], "abc");
            Assert.AreEqual(observableCollection[1], "bCa");
            Assert.AreEqual(observableCollection[2], "caB");
        }

        [TestMethod]
        public void TestMethod2()
        {
            var observableCollection = new ObservableCollection<string> { "bCa", "abc", "caB" };

            observableCollection.SortByDescending(i => i);

            Assert.AreEqual(observableCollection[2], "abc");
            Assert.AreEqual(observableCollection[1], "bCa");
            Assert.AreEqual(observableCollection[0], "caB");
        }

        [TestMethod]
        public void TestMethod3()
        {
            var observableCollection = new ObservableCollection<int> { 34, -2, 0 };

            observableCollection.SortBy(i => i);

            Assert.AreEqual(observableCollection[0], -2);
            Assert.AreEqual(observableCollection[1], 0);
            Assert.AreEqual(observableCollection[2], 34);
        }

        [TestMethod]
        public void TestMethod4()
        {
            var observableCollection = new ObservableCollection<int> { 34, -2, 0 };

            observableCollection.SortByDescending(i => i);

            Assert.AreEqual(observableCollection[2], -2);
            Assert.AreEqual(observableCollection[1], 0);
            Assert.AreEqual(observableCollection[0], 34);
        }
    }
}
