using System.Data;

namespace MCDA.Model
{
    internal static class ToolFactory
    {

        public static WLCTool NewWLCTool()
        {
            var wlcParameter = MCDA.MCDAExtension.GetExtension().GetToolParameterBasedOnSelectedFields();
            wlcParameter.DistributeEquallyToolParameterWeights();

            var dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(wlcParameter.ToolParameter);

            var wlcTool = new WLCTool(dataTable,wlcParameter);

            wlcTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(wlcTool.DefaultResultColumnName);

            return wlcTool;
        }

        public static OWATool NewOWATool()
        {
            var toolParameter =  MCDA.MCDAExtension.GetExtension().GetToolParameterBasedOnSelectedFields();
            toolParameter.DistributeEquallyToolParameterWeights();

            var dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(toolParameter.ToolParameter);

            var owaTool = new OWATool(dataTable, toolParameter);

            owaTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(owaTool.DefaultResultColumnName);

            return owaTool;
        }

        public static LWLCTool NewLWLCTool()
        {
            var toolParameter = MCDA.MCDAExtension.GetExtension().GetToolParameterBasedOnSelectedFields();
            toolParameter.DistributeEquallyToolParameterWeights();

            var dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(toolParameter.ToolParameter);

            var lwlcTool = new LWLCTool(dataTable,toolParameter, MCDA.MCDAExtension.GetExtension().SelectedFeature == null ? null : MCDA.MCDAExtension.GetExtension().SelectedFeature.FeatureClass);

            lwlcTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(lwlcTool.DefaultResultColumnName);

            return lwlcTool;
        }
    }
}
