using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;

namespace MCDA
{
    public class AddDataButton : ESRI.ArcGIS.Desktop.AddIns.Button
    {
        protected override void OnClick()
        {
             UID dockWinID = new UIDClass();
             dockWinID.Value = ThisAddIn.IDs.AddDataView;

            IDockableWindow dockableWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
            
            dockableWindow.Show(true);
            
        }
       
    }

}
