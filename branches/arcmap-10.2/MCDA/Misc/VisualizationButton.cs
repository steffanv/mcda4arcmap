using System;
using System.Windows.Interop;

namespace MCDA
{
    internal sealed class VisualizationButton : ESRI.ArcGIS.Desktop.AddIns.Button
    { 
        protected override void OnClick()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var visualizationView = new VisualizationView();

            var helper = new WindowInteropHelper(visualizationView) {Owner = parentHandle};

            visualizationView.ShowDialog();
        }
    }
}
