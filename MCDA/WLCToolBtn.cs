using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.Windows.Interop;


namespace MCDA
{
    public class WLCToolBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public WLCToolBtn()
        {
        }

        protected override void OnClick()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var wpfWindow = new WLCToolView();

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Show();
        }

        

        protected override void OnUpdate()
        {
        }
    }
}
