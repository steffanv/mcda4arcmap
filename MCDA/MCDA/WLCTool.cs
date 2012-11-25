using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;
using System.Data;
using MCDA.Entity;
using System.Threading.Tasks;
using System.Diagnostics;

namespace MCDA.Model
{
    public class WLCTool : AbstractToolTemplate
    {
        private DataTable _workingDataTable, _backupDataTable;
        private WLCParameter _wlcParameter;
        private TransformationStrategy _transformationStrategy;

        private string _wlcResultColumnName = "WLCResult";
        
        public WLCTool(DataTable dataTable, WLCParameter wlcParameter)
        {
            _backupDataTable = dataTable.Copy();

            _workingDataTable = _backupDataTable;

            _wlcParameter = wlcParameter;

            _transformationStrategy = Model.TransformationStrategy.MaximumScoreTransformationStrategy;
        }

        /// <summary>
        /// If the tool is set to locked changes from the outside like the selected layer which impact the result column name are
        /// no longer possible.
        /// </summary>
        public bool IsLocked { get; set; }

        public DataTable Data
        {
            get { return _workingDataTable.Copy(); }
        }

        public WLCParameter WLCParameter
        {
            get { return _wlcParameter; }
            set { _wlcParameter = value; }
        }

        public TransformationStrategy TransformationStrategy
        {
            get { return _transformationStrategy; }
            set { _transformationStrategy = value; }
        }

        public IWeightDistributionStrategy WeightDistributionStrategy
        {

            get { return _wlcParameter.WeightDistributionStrategy; }
            set { _wlcParameter.WeightDistributionStrategy = value; }

        }

        protected override void PerformPreConditions()
        {
            _workingDataTable = _backupDataTable.Copy();

            //the tool does no react on ouside changes like new fields, because the result column name is set
            //this is important because it has to be the same as in the in memory workspace to perform the join
            if (!IsLocked)
            {
                string oldResultColumnName = _wlcResultColumnName;
                //if name changed remove the old column and add a new one
                string newResultColumnName = MCDA.MCDAExtension.GetExtension().GetSuggestNameForResultColumn(DefaultResultColumnName);

                //nothing changes and the result column exist
                if (newResultColumnName.Equals(oldResultColumnName) && _workingDataTable.Columns.IndexOf(oldResultColumnName) != -1)
                    return;

                //remove old column
                if (_workingDataTable.Columns.IndexOf(oldResultColumnName) != -1)
                {

                    _workingDataTable.Columns.Remove(oldResultColumnName);
                }

                //add new one
                DefaultResultColumnName = newResultColumnName;

                _workingDataTable.Columns.Add(new DataColumn(DefaultResultColumnName, typeof(double)));
            }
  
        }

        protected override void PerformScaling()
        {
            if (_workingDataTable.Rows.Count == 0 || _wlcParameter.ToolParameter.Count == 0)
                return;
            
            foreach(WLCToolParameter currentToolParameter in _wlcParameter.ToolParameter){

               TransformationStrategyFactory.GetStrategy(_transformationStrategy).Transform(_workingDataTable.Columns[currentToolParameter.ColumnName], currentToolParameter.IsBenefitCriterion);
            }
           
        }

        private void AddWeightsToData(DataTable dataTable)
        {
            foreach (WLCToolParameter currentToolParameter in _wlcParameter.ToolParameter)
            {
                int columnIndex = dataTable.Columns.IndexOf(currentToolParameter.ColumnName);

                foreach (DataRow currentDataRow in dataTable.Rows)
                {
                    currentDataRow[columnIndex] = Math.Round(Convert.ToDouble(currentDataRow.ItemArray[columnIndex]) * currentToolParameter.ScaledWeight, 10);
                }
            }

            CalculateResult(dataTable);
        }

        private void CalculateResult(DataTable dataTable) {

            int wlcRankIndex = _workingDataTable.Columns.IndexOf(_wlcResultColumnName);

            foreach (DataRow currentDataRow in dataTable.Rows)
                 {
                
                     double sum = currentDataRow.ItemArray.Where(o => o.GetType() == typeof(double)).Sum(o => (double)o);

                     //the trick is that the result table is still without a value? or at least 0 for the result column
                      //and 0 is the neutral element for the + operator
                     currentDataRow[wlcRankIndex] = sum;
                 }
        
        }

        protected override void PerformAlgorithm()
        {
            if (_workingDataTable.Rows.Count == 0 || _wlcParameter.ToolParameter.Count == 0)
                return;

            Stopwatch sw = Stopwatch.StartNew();
            
            //DataTable schema = _workingDataTable.Clone();
            //DataTable schema2 = _workingDataTable.Clone();

            IList<DataTable> listOfDataTables = new List<DataTable>();

            for(int i = 0; i < _workingDataTable.Rows.Count; i+=100){

                DataTable temp = _workingDataTable.Clone();
               IEnumerable<DataRow> tempDataRow = _workingDataTable.Select().Skip(i).Take(100);

               tempDataRow.CopyToDataTable(temp, LoadOption.OverwriteChanges);

               listOfDataTables.Add(temp);
            }

            /*
            IEnumerable<DataRow> dataRows = _workingDataTable.Select().Take(_workingDataTable.Rows.Count / 2);
            IEnumerable<DataRow> dataRows2 = _workingDataTable.Select().Skip(_workingDataTable.Rows.Count / 2).Take(_workingDataTable.Rows.Count -( _workingDataTable.Rows.Count / 2));

            dataRows.CopyToDataTable(schema, LoadOption.OverwriteChanges);
            dataRows2.CopyToDataTable(schema2, LoadOption.OverwriteChanges);

            listOfDataTables.Add(schema);
            listOfDataTables.Add(schema2);
            */

            Parallel.ForEach(listOfDataTables, dataTable => AddWeightsToData(dataTable));
            
            /*
            foreach(WLCToolParameter currentToolParameter in _wlcParameter.ToolParameter){

                int columnIndex = _workingDataTable.Columns.IndexOf(currentToolParameter.ColumnName);

                foreach (DataRow currentDataRow in _workingDataTable.Rows)
                {
                    currentDataRow[columnIndex] = Math.Round( Convert.ToDouble(currentDataRow.ItemArray[columnIndex]) * currentToolParameter.ScaledWeight,10);
                }
            }
            

            int wlcRankIndex = _workingDataTable.Columns.IndexOf(_wlcResultColumnName);
             
            //we ensure that the oid is not part of the calculation by using only columns with the type double
            //there is a custon OID type for OIDs

            
            foreach (DataRow currentDataRow in _workingDataTable.Rows)
                 {
                
                     double sum = currentDataRow.ItemArray.Where(o => o.GetType() == typeof(double)).Sum(o => (double)o);

                     //the trick is that the result table is still without a value? or at least 0 for the result column
                      //and 0 is the neutral element for the + operator
                     currentDataRow[wlcRankIndex] = sum;
                 }
             
            */

            DataTable targetSchema = _workingDataTable.Clone();

            for (int i = 0; i < listOfDataTables.Count; i++)
            {
                targetSchema.Merge(listOfDataTables[i]);
            }


            _workingDataTable = targetSchema;

            sw.Stop();

            Console.WriteLine("Time taken: {0}ms", sw.Elapsed.TotalMilliseconds);
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
