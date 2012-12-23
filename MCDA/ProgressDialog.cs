using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.ViewModel
{
    static class ProgressDialog
    {
        public static void ShowProgressDialog<T>(string message, Action<T> action, T arg)
        {
           
             // Create a CancelTracker
            ESRI.ArcGIS.esriSystem.ITrackCancel trackCancel = new ESRI.ArcGIS.Display.CancelTrackerClass();

            ESRI.ArcGIS.Framework.IProgressDialogFactory progressDialogFactory = new ESRI.ArcGIS.Framework.ProgressDialogFactoryClass();

            // Set the properties of the Step Progressor
            System.Int32 int32_hWnd = ArcMap.Application.hWnd;
            ESRI.ArcGIS.esriSystem.IStepProgressor stepProgressor = progressDialogFactory.Create(trackCancel, int32_hWnd);

            //stepProgressor.MinRange = 1;
            //stepProgressor.MaxRange = 4;
            //stepProgressor.StepValue = 1;
            //stepProgressor.Message = message;

            

            // Create the ProgressDialog. This automatically displays the dialog
            ESRI.ArcGIS.Framework.IProgressDialog2 progressDialog2 = (ESRI.ArcGIS.Framework.IProgressDialog2)stepProgressor; // Explict Cast

            // Set the properties of the ProgressDialog
            progressDialog2.CancelEnabled = false;
            progressDialog2.Description = message;
            //progressDialog2.Title = "Counting...";
            progressDialog2.Animation = ESRI.ArcGIS.Framework.esriProgressAnimationTypes.esriProgressSpiral;

            //stepProgressor.StepValue = 2;
            action.Invoke(arg);

            //stepProgressor.StepValue = 4;

            trackCancel = null;
            //stepProgressor = null;
            progressDialog2.HideDialog();
            progressDialog2 = null;

        }
    }
}
