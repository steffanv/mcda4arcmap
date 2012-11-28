﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Entity;
using System.Data;

namespace MCDA.Model
{
    public static class ToolFactory
    {

        public static AbstractToolTemplate NewWLCTool()
        {
            ToolParameterContainer wlcParameter = MCDA.MCDAExtension.GetExtension().GetWLCParameterBasedOnSelectedFields();
            DataTable dataTable = MCDA.MCDAExtension.GetExtension().GetDataTableForParameterSet(wlcParameter.ToolParameter);

            WLCTool wlcTool = new WLCTool(dataTable,wlcParameter);

            wlcTool.DefaultResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(wlcTool.DefaultResultColumnName);

            return wlcTool;
        }

        public static AbstractToolTemplate NewOWATool()
        {
            return null;
        }
    }

    public enum Tool
    {
        WLCTool
    }
}
