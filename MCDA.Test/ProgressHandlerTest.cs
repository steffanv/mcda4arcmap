using System;
using System.ComponentModel;
using MCDA.Misc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MCDA.Test
{
    [TestClass]
    public class ProgressHandlerTest
    {
        private void Task(ProgressHandler progressHandler)
        {
            //progressHandler.Callback(1, "blub");
        }

        public void ProgressDialogMockUp(Action<ProgressHandler> action)
        {
            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };

            Delegate LWCLAction = action;

            worker.DoWork += (s, ea) =>
            {
                Action<int, string> callback = new Action<int, string>((p, t) =>
                {
                    (s as BackgroundWorker).ReportProgress(p);
                    string str = t;

                }
                );

                ProgressHandler progressHandler = new ProgressHandler();

                LWCLAction.DynamicInvoke(progressHandler);

            };
            worker.ProgressChanged += (s, ex) =>
            {
                //dlg.ProgressBar.Value = ex.ProgressPercentage;
                int i = ex.ProgressPercentage;
            };

            worker.RunWorkerCompleted += (o, ea) =>
            {
                //dlg.Close();
            };

            //dlg.Show();
            worker.RunWorkerAsync();
        }
        [TestMethod]
        public void TestMethod1()
        {
            ProgressDialogMockUp(Task);
            //Assert.AreEqual(1, 2);
        }

        [TestMethod]
        public void TestMethod2()
        {
            ProgressHandler progressHandler = new ProgressHandler();

            for (int i = 0; i < 100; i++)
            {
                progressHandler.OnProgress(i, i.ToString());

                Assert.AreEqual(i, progressHandler.Progress);
                Assert.AreEqual(i.ToString(), progressHandler.Text);
            }
        }

        [TestMethod]
        public void TestMethod3()
        {
            ProgressHandler progressHandler = new ProgressHandler();
            string progressText = "50 percent";
            int progress = 0;
            string text = String.Empty;

            progressHandler.ProgressEvent += (p, t) =>
            {
                progress = p;
                text = t;
            };

            progressHandler.OnProgress(50, progressText);

            Assert.AreEqual(50, progress);
            Assert.AreEqual(text, progressText);
        }

        [TestMethod]
        public void TestMethod4()
        {
            ProgressHandler progressHandler = new ProgressHandler();

            progressHandler.OnProgress(50, String.Empty);

            ProgressHandler childHandler = progressHandler.ProvideChildProgressHandler(10);
            childHandler.OnProgress(50, String.Empty);

            Assert.AreEqual(55, progressHandler.Progress);

            ProgressHandler childCallback2 = progressHandler.ProvideChildProgressHandler(40);

            // Getting a new one finishes the old one
            Assert.AreEqual(60, progressHandler.Progress);
        }
    }
}
