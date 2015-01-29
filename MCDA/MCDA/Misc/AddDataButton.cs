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

            var dockableWindow = ArcMap.DockableWindowManager.GetDockableWindow(dockWinID);
            
            dockableWindow.Show(true);
            
        }
       
    }

}
