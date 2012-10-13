using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;


namespace MCDA
{
    public class WLCToolBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public WLCToolBtn()
        {
        }

        protected override void OnClick()
        {
            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.WLCToolView;

            IDockableWindow w = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);

            w.Show(true);
        }

        protected override void OnUpdate()
        {
        }
    }
}
