using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;
using System.Data;
using MCDA.Entity;

namespace MCDA.Model
{
    public class WLCTool : AbstractToolTemplate
    {
        private DataTable _workingDataTable, _backupDataTable;
        private WLCParameter _wlcParameter;
        private ITransformationStrategy _transformationStrategy;

        private const string _wlcResultColumnName = "WLCResult";
        
        public WLCTool(DataTable dataTable, WLCParameter wlcParameter)
        {
            _backupDataTable = dataTable.Copy();

            _workingDataTable = _backupDataTable;

            _wlcParameter = wlcParameter; 

            _transformationStrategy = TransformationStrategyFactory.DefaultTransformationStrategy();
        }

        public DataTable Data
        {
            get { return _workingDataTable.Copy(); }
            set { ;}
        }

        public WLCParameter WLCParameter
        {
            get { return _wlcParameter; }
            set { _wlcParameter = value; }
        }

        public ITransformationStrategy TransformationStrategy
        {
            get { return _transformationStrategy; }
            set { _transformationStrategy = value; }
        }

        public IWeightDistributionStrategy WeightDistributionStrategy
        {

            get { return _wlcParameter.WeightDistributionStrategy; }
            set { _wlcParameter.WeightDistributionStrategy = value; }

        }

        protected override void PerformScaling()
        {
            _workingDataTable = _backupDataTable.Copy();

            foreach(WLCToolParameter currentToolParameter in _wlcParameter.ToolParameter){

                _transformationStrategy.Transform(_workingDataTable.Columns[currentToolParameter.ColumnName], currentToolParameter.IsBenefitCriterion);
            }
           
        }

        protected override void PerformAlgorithm()
        {
            foreach(WLCToolParameter currentToolParameter in _wlcParameter.ToolParameter){

                int columnIndex = _workingDataTable.Columns.IndexOf(currentToolParameter.ColumnName);

                foreach (DataRow currentDataRow in _workingDataTable.Rows)
                {
                    currentDataRow[columnIndex] = Math.Round( Convert.ToDouble(currentDataRow.ItemArray[columnIndex]) * currentToolParameter.ScaledWeight,10);
                }
            }

            int wlcRankIndex = 0;
            if (_workingDataTable.Columns.Contains(_wlcResultColumnName))
            {
                wlcRankIndex = _workingDataTable.Columns.IndexOf(_wlcResultColumnName);

            }
            else
            {
                _workingDataTable.Columns.Add(new DataColumn(_wlcResultColumnName, typeof(double)));
                wlcRankIndex = _workingDataTable.Columns.IndexOf(_wlcResultColumnName);
            }


            //TODO this is tooo scary I schould fix this.

            //int oidIndex = _workingDataTable.Columns.IndexOf("OBJECTID");

            //DataTable dtWithoutOID = _workingDataTable.Copy();

            foreach (DataRow currentDataRow in _workingDataTable.Rows)
                 {
                
                     double sum = currentDataRow.ItemArray.Where(o => o.GetType() == typeof(double)).Sum(o => (double)o);

                     currentDataRow[wlcRankIndex] = sum;
                 }
        }

        public override string ToString(){

            return "WLC Tool";
        }


        public override string DefaultResultColumnName
        {
            get { return _wlcResultColumnName; }
        }
    }
}
