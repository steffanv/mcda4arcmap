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

            //it makes sense to split the table to work parallel
            //it is likely that it would make even more sense to split the table into 4 or even more sub tables, 
            //however it have only 2 cores... and no idea about the required table size
            if (workingDataTable.Rows.Count >= 500 && toolParameterContainer.ToolParameter.Count > 6)
            {

                DataTable tableOne = workingDataTable.Clone();
                DataTable tableTwo = workingDataTable.Clone();

                IEnumerable<DataRow> dataRowsOne = workingDataTable.Select().Take(workingDataTable.Rows.Count / 2);
                IEnumerable<DataRow> dataRowsTwo = workingDataTable.Select().Skip(workingDataTable.Rows.Count / 2).Take(workingDataTable.Rows.Count);

                dataRowsOne.CopyToDataTable(tableOne, LoadOption.OverwriteChanges);
                dataRowsTwo.CopyToDataTable(tableTwo, LoadOption.OverwriteChanges);

                Parallel.Invoke(() => RunWLC(tableOne), () => RunWLC(tableTwo));

                DataTable targetSchema = workingDataTable.Clone();

                targetSchema.Merge(tableOne);
                targetSchema.Merge(tableTwo);

                workingDataTable = targetSchema;
            }

            else
            {
                RunWLC(workingDataTable);
            }
                  
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
