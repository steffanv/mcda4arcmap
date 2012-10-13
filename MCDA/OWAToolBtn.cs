using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;


namespace MCDA
{
    public class OWAToolBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public OWAToolBtn()
        {
        }

        protected override void OnClick()
        {

            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.OWAToolView;

            IDockableWindow w = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);

            w.Show(true);
        }

        protected override void OnUpdate()
        {
        }
    }
}
