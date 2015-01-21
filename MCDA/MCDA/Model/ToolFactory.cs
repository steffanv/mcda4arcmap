﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    internal static class ToolFactory
    {

        public static WLCTool NewWLCTool()
        {
            ToolParameterContainer wlcParameter = MCDA.MCDAExtension.GetExtension().GetToolParameterBasedOnSelectedFields();
            wlcParameter.DistributeEquallyToolParameterWeights();

            DataTable dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(wlcParameter.ToolParameter);

            WLCTool wlcTool = new WLCTool(dataTable,wlcParameter);

            wlcTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(wlcTool.DefaultResultColumnName);

            return wlcTool;
        }

        public static OWATool NewOWATool()
        {
            ToolParameterContainer toolParameter =  MCDA.MCDAExtension.GetExtension().GetToolParameterBasedOnSelectedFields();
            toolParameter.DistributeEquallyToolParameterWeights();

            DataTable dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(toolParameter.ToolParameter);

            OWATool owaTool = new OWATool(dataTable, toolParameter);

            owaTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(owaTool.DefaultResultColumnName);

            return owaTool;
        }

        public static LWLCTool NewLWLCTool()
        {
            ToolParameterContainer toolParameter = MCDA.MCDAExtension.GetExtension().GetToolParameterBasedOnSelectedFields();
            toolParameter.DistributeEquallyToolParameterWeights();

            DataTable dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(toolParameter.ToolParameter);

            LWLCTool lwlcTool = new LWLCTool(dataTable,toolParameter, MCDA.MCDAExtension.GetExtension().SelectedFeature == null ? null : MCDA.MCDAExtension.GetExtension().SelectedFeature.FeatureClass);

            lwlcTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(lwlcTool.DefaultResultColumnName);

            return lwlcTool;
        }
    }
}