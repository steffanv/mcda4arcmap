using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace MCDA
{
    public class AddDataBtn : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        public AddDataBtn()
        {
        }

        protected override void OnClick()
        {
             UID dockWinID = new UIDClass();
             dockWinID.Value = ThisAddIn.IDs.AddDataView;

            IDockableWindow w = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
            
            w.Show(true);
            
            //ArcMap.Application.CurrentTool = null;
        }
        protected override void OnUpdate()
        {
            //Enabled = ArcMap.Application != null;
        }
    }

}
