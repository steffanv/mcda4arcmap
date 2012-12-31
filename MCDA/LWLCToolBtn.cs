using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Interop;


namespace MCDA
{
    public class LWLCToolBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public LWLCToolBtn()
        {
        }

        protected override void OnClick()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var wpfWindow = new LWLCToolView();

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Show();
        }

        protected override void OnUpdate()
        {
        }
    }
}
