using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.ViewModel
{
    internal static class ProgressDialog
    {
        public static void ShowProgressDialog(string message, Delegate action, params object[] args)
        {          
             // Create a CancelTracker
            ESRI.ArcGIS.esriSystem.ITrackCancel trackCancel = new ESRI.ArcGIS.Display.CancelTrackerClass();

            ESRI.ArcGIS.Framework.IProgressDialogFactory progressDialogFactory = new ESRI.ArcGIS.Framework.ProgressDialogFactoryClass();

            // Set the properties of the Step Progressor
            System.Int32 int32_hWnd = ArcMap.Application.hWnd;
            ESRI.ArcGIS.esriSystem.IStepProgressor stepProgressor = progressDialogFactory.Create(trackCancel, int32_hWnd);
            stepProgressor.Hide();

            // Create the ProgressDialog. This automatically displays the dialog
            ESRI.ArcGIS.Framework.IProgressDialog2 progressDialog2 = (ESRI.ArcGIS.Framework.IProgressDialog2)stepProgressor;

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
