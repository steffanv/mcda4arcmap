using System;
using System.Linq;
using System.Data;
using MCDA.Misc;

namespace MCDA.Model
{
   internal sealed class WLCTool : AbstractToolTemplate
    {
        private DataTable _workingDataTable;
        private readonly DataTable _backupDataTable;
        private ToolParameterContainer _toolParameterContainer;
        private NormalizationStrategy _transformationStrategy;

        private string _wlcResultColumnName = "WLCResult";
        
        public WLCTool(DataTable dataTable, ToolParameterContainer toolParameterContainer)
        {

            _backupDataTable = dataTable.Copy();

            _workingDataTable = _backupDataTable;

            this._toolParameterContainer = toolParameterContainer;

            _transformationStrategy = Model.NormalizationStrategy.MaximumScoreNormalizationStrategy;
        }

        public override DataTable Data
        {
            get { return _workingDataTable.Copy(); }
        }

        public override ToolParameterContainer ToolParameterContainer
        {
            get { return _toolParameterContainer; }
            set { _toolParameterContainer = value; }
        }

        public override NormalizationStrategy TransformationStrategy
        {
            get { return _transformationStrategy; }
            set { _transformationStrategy = value; }
        }

        protected override void PerformScaling()
        {
            _workingDataTable = _backupDataTable.Copy();

            foreach (var currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                NormalizationStrategyFactory.GetStrategy(_transformationStrategy)
                    .Transform(_workingDataTable.Columns[currentToolParameter.ColumnName],
                        currentToolParameter.IsBenefitCriterion);
            }


        }

        private void RunWLC(DataTable dataTable)
        {
            foreach (var currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                var columnIndex = dataTable.Columns.IndexOf(currentToolParameter.ColumnName);

                foreach (DataRow currentDataRow in dataTable.Rows)
                {
                    currentDataRow[columnIndex] = Convert.ToDouble(currentDataRow.ItemArray[columnIndex])*
                                                  currentToolParameter.ScaledWeight;
                }
            }

            CalculateResult(dataTable);
        }

        private void CalculateResult(DataTable dataTable)
        {

            var wlcRankIndex = dataTable.Columns.IndexOf(_wlcResultColumnName);

            foreach (DataRow currentDataRow in dataTable.Rows)
            {
                var sum = currentDataRow.ItemArray.Where(o => o is double).Sum(o => (double)o);

                //the trick is that the result table is still without a value? or at least 0 for the result column
                //and 0 is the neutral element for the + operator
                currentDataRow[wlcRankIndex] = Math.Round(sum, 6);
            }

        }

        protected override void PerformAlgorithm(ProgressHandler childHandler = null)
        {   
            //add result column
            _workingDataTable.Columns.Add(new DataColumn(DefaultResultColumnName, typeof(double)));

            if (_workingDataTable.Rows.Count >= 2000 && _toolParameterContainer.ToolParameter.Count > 5)
            {
                _workingDataTable = PerformAlgorithmInParallel(_workingDataTable, RunWLC);
            }

            else
            {
                RunWLC(_workingDataTable);
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
