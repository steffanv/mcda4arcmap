using System;
using System.Windows.Interop;


namespace MCDA
{
    internal sealed class ConfigButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {  
        protected override void OnClick()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var configView = new ConfigView();

            var helper = new WindowInteropHelper(configView) {Owner = parentHandle};

            configView.ShowDialog();
        }
    }
}
