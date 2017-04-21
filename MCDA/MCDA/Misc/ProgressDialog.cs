using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Threading;

namespace MCDA.Misc
{
    internal sealed class ProgressDialogBuilder
    {
        private readonly ProgressDialogBuilder _progressDialogBuilder;
        private string Title { get; set; }
        private Delegate ActionDelegate { get; set; }
        private object[] Params { get; set; }
        private bool IsIndeterminate { get; set; }
        private bool HideCancelButton { get; set; }

        private ProgressDialogBuilder()
        {
        }

        public ProgressDialogBuilder(Delegate action, params object[] args)
        {
            _progressDialogBuilder = new ProgressDialogBuilder { ActionDelegate = action, Params = args };
        }

        public ProgressDialogBuilder(Action<ProgressHandler> action)
        {
            _progressDialogBuilder = new ProgressDialogBuilder { ActionDelegate = action };
        }

        public ProgressDialogBuilder SetTitle(string title)
        {
            _progressDialogBuilder.Title = title;
            return this;
        }

        public ProgressDialogBuilder SetIsIndeterminate(bool indeterminate)
        {
            _progressDialogBuilder.IsIndeterminate = indeterminate;
            return this;
        }

        public ProgressDialogBuilder SetHideCancelButton(bool hideCancelButton)
        {
            _progressDialogBuilder.HideCancelButton = hideCancelButton;
            return this;
        }
        public ProgressDialog Build()
        {
            ProgressDialog progressDialog = new ProgressDialog
            {
                Title = _progressDialogBuilder.Title,
                ActionDelegate = _progressDialogBuilder.ActionDelegate,
                HideCancelButton = _progressDialogBuilder.HideCancelButton,
                IsIndetermiante = _progressDialogBuilder.IsIndeterminate,
                Params = _progressDialogBuilder.Params
            };
            return progressDialog;
        }
    }
    internal sealed class ProgressDialog
    {
        public string Title { private get; set; }

        public Delegate ActionDelegate { private get; set; }

        public object [] Params { get; set; }

        public bool HideCancelButton { private get; set; }

        public bool IsIndetermiante { private get; set; }

        public void Show()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);
            WpfProgressDialog progressDialog = new WpfProgressDialog
            {
                Title = Title,
                HideCancelButton = HideCancelButton,
                IsIndeterminate = IsIndetermiante
            };
            var windowInteropHelper = new WindowInteropHelper(progressDialog) { Owner = parentHandle };

            BackgroundWorker worker = new BackgroundWorker { WorkerReportsProgress = true };

            if (ActionDelegate is Action<ProgressHandler>)
            {
                worker.DoWork += (s, ea) =>
                {

                    ProgressHandler progressHandler = new ProgressHandler();
                    progressHandler.ProgressEvent += (progress, text) =>
                    {
                        (s as BackgroundWorker).ReportProgress(progress);
                        progressDialog.ProgressText.Dispatcher.Invoke(
                            () => { progressDialog.ProgressText.Content = text; },
                            DispatcherPriority.ContextIdle);
                    };

                    //var workerThread = new Thread(new ThreadStart(() => {

                    //    // to update the collection, we need to do it from within the Dispatcher's thread, because of thread affinity.
                    //    var item = DoOperationThatCallsIEComObjectAndReturnsItem();
                    //    Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Send, addItem, item);
                    //}));
                    //workerThread.SetApartmentState(ApartmentState.STA); // set ApartmentState to STA
                    //workerThread.Start(); // start it

                    ActionDelegate.DynamicInvoke(progressHandler);
                };

                worker.ProgressChanged += (s, ex) => progressDialog.ProgressBar.Value = ex.ProgressPercentage;
                worker.RunWorkerCompleted += (o, ea) => progressDialog.Close();
                worker.RunWorkerAsync();
                progressDialog.ShowDialog();
            }
            else
            {

                //var workerThread = new Thread(new ThreadStart(() =>
                //{
                //    ActionDelegate.DynamicInvoke(Params);
                //    progressDialog.Close();
                //}));
                //workerThread.SetApartmentState(ApartmentState.STA);
                //workerThread.Start();

                //progressDialog.ShowDialog();

                Task<object> TaskLoader;
                TaskLoader = Task<object>.Factory.StartNew(() =>
                {
                    ActionDelegate.DynamicInvoke(Params);
                    progressDialog.SetNativeEnabled(true);
                    progressDialog.Show();
                    return null;
                    
                });

                TaskLoader.ContinueWith(t => { progressDialog.Close();},
                    TaskScheduler.FromCurrentSynchronizationContext());

            }         
        }

        public static void ShowProgressDialog(string message, Delegate action, params object[] args)
        {          
             // Create a CancelTracker
            ESRI.ArcGIS.esriSystem.ITrackCancel trackCancel = new ESRI.ArcGIS.Display.CancelTrackerClass();

            ESRI.ArcGIS.Framework.IProgressDialogFactory progressDialogFactory = new ESRI.ArcGIS.Framework.ProgressDialogFactoryClass();

            // Set the properties of the Step Progressor
            var int32_hWnd = ArcMap.Application.hWnd;
            var stepProgressor = progressDialogFactory.Create(trackCancel, int32_hWnd);
            stepProgressor.Hide();

            // Create the ProgressDialog. This automatically displays the dialog
            var progressDialog2 = (ESRI.ArcGIS.Framework.IProgressDialog2)stepProgressor;

            // Set the properties of the ProgressDialog
            progressDialog2.CancelEnabled = false;
            progressDialog2.Description = message;
            progressDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

            progressDialog2.ShowDialog();

            progressDialog2.CancelEnabled = false;

            action.DynamicInvoke(args);

            progressDialog2.CancelEnabled = false;

            trackCancel = null;
            stepProgressor = null;
            progressDialog2.HideDialog();
            progressDialog2 = null;

        }
    }
}
