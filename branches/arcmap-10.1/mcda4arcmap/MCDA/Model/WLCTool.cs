using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MCDA.Model
{
   internal sealed class WLCTool : AbstractToolTemplate
    {

        private DataTable workingDataTable, backupDataTable;
        private ToolParameterContainer toolParameterContainer;
        private NormalizationStrategy transformationStrategy;

        private string _wlcResultColumnName = "WLCResult";
        
        public WLCTool(DataTable dataTable, ToolParameterContainer toolParameterContainer)
        {

            backupDataTable = dataTable.Copy();

            workingDataTable = backupDataTable;

            this.toolParameterContainer = toolParameterContainer;

            transformationStrategy = Model.NormalizationStrategy.MaximumScoreNormalizationStrategy;
        }

        public override DataTable Data
        {
            get { return workingDataTable.Copy(); }
        }

        public override ToolParameterContainer ToolParameterContainer
        {
            get { return toolParameterContainer; }
            set { toolParameterContainer = value; }
        }

        public override NormalizationStrategy TransformationStrategy
        {
            get { return transformationStrategy; }
            set { transformationStrategy = value; }
        }

        protected override void PerformScaling()
        {
            workingDataTable = backupDataTable.Copy();

            foreach(IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
               NormalizationStrategyFactory.GetStrategy(transformationStrategy).Transform(workingDataTable.Columns[currentToolParameter.ColumnName], currentToolParameter.IsBenefitCriterion);
          
           
        }

        private void RunWLC(DataTable dataTable)
        {
            foreach (IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
            {
                int columnIndex = dataTable.Columns.IndexOf(currentToolParameter.ColumnName);

                foreach (DataRow currentDataRow in dataTable.Rows)
                    currentDataRow[columnIndex] = Convert.ToDouble(currentDataRow.ItemArray[columnIndex]) * currentToolParameter.ScaledWeight;

            }

            CalculateResult(dataTable);
        }

        private void CalculateResult(DataTable dataTable) {

            int wlcRankIndex = dataTable.Columns.IndexOf(_wlcResultColumnName);

            foreach (DataRow currentDataRow in dataTable.Rows)
                 {          
                     double sum = currentDataRow.ItemArray.Where(o => o is double).Sum(o => (double)o);

                     //the trick is that the result table is still without a value? or at least 0 for the result column
                      //and 0 is the neutral element for the + operator
                     currentDataRow[wlcRankIndex] = Math.Round(sum,6);
                 }
        
        }

        protected override void PerformAlgorithm()
        {   
            //add result column
            workingDataTable.Columns.Add(new DataColumn(DefaultResultColumnName, typeof(double)));

            if (workingDataTable.Rows.Count >= 2000 && toolParameterContainer.ToolParameter.Count > 5)
               workingDataTable = base.PerformAlgorithmInParallel(workingDataTable, RunWLC);

            else
                RunWLC(workingDataTable);
         
        }

        public override string ToString(){

            return "WLC Tool";
        }

        public override string DefaultResultColumnName
        {
            get { return _wlcResultColumnName; }
            set { _wlcResultColumnName = value; }
        }
   
    }
}
