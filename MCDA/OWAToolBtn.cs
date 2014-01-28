using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.Windows.Interop;


namespace MCDA
{
    internal sealed class OWAToolBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public OWAToolBtn()
        {
        }

        protected override void OnClick()
        {

            //UID dockWinID = new UIDClass();
            //dockWinID.Value = ThisAddIn.IDs.OWAToolView;

            //IDockableWindow w = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);

            //w.Show(true);

            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            var wpfWindow = new OWAToolView();

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Show();
        }

        protected override void OnUpdate()
        {
        }
    }
}
