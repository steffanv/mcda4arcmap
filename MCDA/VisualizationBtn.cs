using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;


namespace MCDA
{
    public class VisualizationBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public VisualizationBtn()
        {
        }

        protected override void OnClick()
        {

            UID dockWinID = new UIDClass();
            dockWinID.Value = ThisAddIn.IDs.VisualizationView;

            IDockableWindow w = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);

            w.Show(true);
        }

        protected override void OnUpdate()
        {
        }
    }
}
