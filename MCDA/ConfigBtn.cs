using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.Framework;
using ESRI.ArcGIS.esriSystem;


namespace MCDA
{
    public class ConfigBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public ConfigBtn()
        {
        }

        protected override void OnClick()
        {
            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.ConfigView;

            IDockableWindow w = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);

            w.Show(true);
        }

        protected override void OnUpdate()
        {
        }
    }
}
