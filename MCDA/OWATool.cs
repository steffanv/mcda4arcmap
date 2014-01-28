using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace MCDA.Model
{
    internal sealed class OWATool : AbstractToolTemplate
    {
        private DataTable _workingDataTable, _backupDataTable;
        private ToolParameterContainer _toolParameterContainer;
        private StandardizationStrategy _transformationStrategy;
        private double _alpha;

        private string _owaResultColumnName = "OWAResult";

        public OWATool(DataTable dataTable, ToolParameterContainer toolParameterContainer)
        {
            _backupDataTable = dataTable.Copy();

            _workingDataTable = _backupDataTable;

            _toolParameterContainer = toolParameterContainer;

            _transformationStrategy = Model.StandardizationStrategy.MaximumScoreStandardizationStrategy;

            _alpha = 1;
        }

        public double Alpha
        {
            get { return _alpha; }
            set { _alpha = value; }
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

        public override StandardizationStrategy TransformationStrategy
        {
            get { return _transformationStrategy; }
            set { _transformationStrategy = value; }
        }

        public override string DefaultResultColumnName
        {
            get { return _owaResultColumnName; }
            set { _owaResultColumnName = value; }
        }

        protected override void PerformScaling()
        {
            _workingDataTable = _backupDataTable.Copy();

            foreach (IToolParameter currentToolParameter in _toolParameterContainer.ToolParameter)
            {

                StandardizationStrategyFactory.GetStrategy(_transformationStrategy).Transform(_workingDataTable.Columns[currentToolParameter.ColumnName], currentToolParameter.IsBenefitCriterion);
            }
        }

        private void RunOWA(DataTable dataTable)
        {
            //first step: value * criterion weight
            foreach (IToolParameter currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                int columnIndex = dataTable.Columns.IndexOf(currentToolParameter.ColumnName);

                foreach (DataRow currentDataRow in dataTable.Rows)
                {
                    currentDataRow[columnIndex] = Convert.ToDouble(currentDataRow.ItemArray[columnIndex]) * currentToolParameter.ScaledWeight;
                }
            }

            //second step: calculate order weights
            double[] orderWeights = new double[_toolParameterContainer.ToolParameter.Count];

            for (int i = 0; i < orderWeights.Length; i++)
            {
                orderWeights[i] = Math.Pow(((double)(i+1)/orderWeights.Length), _alpha) - Math.Pow(((double)(i)/orderWeights.Length) ,_alpha);
            }

            int[] indices = new int[_toolParameterContainer.ToolParameter.Count];

            for (int i = 0; i < _toolParameterContainer.ToolParameter.Count; i++ )
            {
                indices[i] = dataTable.Columns.IndexOf(_toolParameterContainer.ToolParameter[i].ColumnName);
            }
            
            //third step: apply order weights
            foreach (DataRow currentDataRow in dataTable.Rows)
            {
                double[] values = new double[indices.Length];
 
                for (int i = 0; i < indices.Length; i++)
                {
                    values[i] = (double) currentDataRow.ItemArray[indices[i]]; 
                }

                Array.Sort(values, indices);

                //apply order weights
                for (int i = 0; i < values.Length; i++)
                {
                    //values[i] *= orderWeights[i];

                    currentDataRow[indices[i]] = values[i] * orderWeights[values.Length-1 - i];
                }
            }

            CalculateResult(dataTable);
        }

        private void CalculateResult(DataTable dataTable)
        {
            int owaRankIndex = dataTable.Columns.IndexOf(_owaResultColumnName);

            foreach (DataRow currentDataRow in dataTable.Rows)
            {
                double sum = currentDataRow.ItemArray.Where(o => o is double).Sum(o => (double)o);

                //the trick is that the result table is still without a value? or at least 0 for the result column
                //and 0 is the neutral element for the + operator
                currentDataRow[owaRankIndex] = Math.Round(sum, 6);
            }
        }

        protected override void PerformAlgorithm()
        {
            //add result column
            _workingDataTable.Columns.Add(new DataColumn(DefaultResultColumnName, typeof(double)));

            //it makes sense to split the table to work parallel
            //it is likely that it would make even more sense to split the table into 4 or even more sub tables, 
            //however it have only 2 cores... and no idea about the required table size
            if (_workingDataTable.Rows.Count >= 500 && _toolParameterContainer.ToolParameter.Count > 6)
            {

                DataTable tableOne = _workingDataTable.Clone();
                DataTable tableTwo = _workingDataTable.Clone();

                IEnumerable<DataRow> dataRowsOne = _workingDataTable.Select().Take(_workingDataTable.Rows.Count / 2);
                IEnumerable<DataRow> dataRowsTwo = _workingDataTable.Select().Skip(_workingDataTable.Rows.Count / 2).Take(_workingDataTable.Rows.Count);

                dataRowsOne.CopyToDataTable(tableOne, LoadOption.OverwriteChanges);
                dataRowsTwo.CopyToDataTable(tableTwo, LoadOption.OverwriteChanges);

                Parallel.Invoke(() => RunOWA(tableOne), () => RunOWA(tableTwo));

                DataTable targetSchema = _workingDataTable.Clone();

                targetSchema.Merge(tableOne);
                targetSchema.Merge(tableTwo);

                _workingDataTable = targetSchema;
            }

            else
            {
                RunOWA(_workingDataTable);
            }

        }

        public override string ToString()
        {
            return "OWA Tool";
        }
    }
}
