using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;
using MCDA.ViewModel;
using System.Windows.Interop;
using MCDA.Model;
using System.ComponentModel;


namespace MCDA
{
    internal sealed class ConfigBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
       
        protected override void OnClick()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            ConfigView configView = new ConfigView();

            var helper = new WindowInteropHelper(configView);

            helper.Owner = parentHandle;

            configView.Closing += ConfigViewClosing;

            configView.ShowDialog();
        }

        void ConfigViewClosing(object sender, CancelEventArgs e)
        {
            //TODO nothing....so far
           //ConfigSingleton.Instance.SelectedRenderoption = ConfigViewModel.SelectedRenderOption;
        }
    }
}
