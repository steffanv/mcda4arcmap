using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.Windows.Interop;


namespace MCDA
{
    internal sealed class VisualizationButton : ESRI.ArcGIS.Desktop.AddIns.Button
    { 
        protected override void OnClick()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            VisualizationView visualizationView = new VisualizationView();

            var helper = new WindowInteropHelper(visualizationView);

            helper.Owner = parentHandle;

            visualizationView.Show();
        }
    }
}
