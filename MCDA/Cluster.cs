using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    class Cluster
    {
        private ToolParameterContainer _toolParameterContainer;
        private DataTable _dataTable;
        private int _featureID;
        private IList<int> _clusterIDs;

        public Cluster(int featureID, IList<int> clusterIDs, DataTable dt, ToolParameterContainer toolParameterContainer)
        {
            _featureID = featureID;
            _clusterIDs = clusterIDs;
            _dataTable = dt;
            _toolParameterContainer = toolParameterContainer;
        }

        private int GetOIDColumnIndex()
        {
            foreach (DataColumn currentDataColumn in _dataTable.Columns)
            {
                if (currentDataColumn.DataType == typeof(FieldTypeOID))
                    return currentDataColumn.Ordinal;
            }

            return -1;
        }

        private IList<Tuple<IToolParameter, double>> GlobalRange(ToolParameterContainer toolParameterContainer)
        {
            IList<Tuple<IToolParameter, double>> listOfRangeTuple = new List<Tuple<IToolParameter, double>>();

            foreach (IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
            {
                IList<double> data = _dataTable.AsEnumerable().Select(x => x.Field<double>(currentToolParameter.ColumnName)).Distinct().ToList();

                listOfRangeTuple.Add(Tuple.Create(currentToolParameter, data.Max() - data.Min()));
            }

            return listOfRangeTuple;
        }

        private IList<Tuple<IToolParameter, double>> LocalRange(IList<int> clusterIDs, ToolParameterContainer toolParameterContainer)
        {
            int oidColumnIndex = GetOIDColumnIndex();

            IList<Tuple<IToolParameter, double>> listOfRangeTuple = new List<Tuple<IToolParameter, double>>();

            foreach(IToolParameter currentToolParameter in toolParameterContainer.ToolParameter){

                IList<double> data =_dataTable.AsEnumerable().Where(x => _clusterIDs.Contains(x.Field<FieldTypeOID>(oidColumnIndex).OID)).Select(x => x.Field<double>(currentToolParameter.ColumnName)).Distinct().ToList();

                listOfRangeTuple.Add(Tuple.Create(currentToolParameter, data.Max() - data.Min()));

                }

            return listOfRangeTuple;
        }

        private  IList<Tuple<IToolParameter, double>> Scale(ToolParameterContainer toolParameterContainer, IList<Tuple<IToolParameter, double>> localRangeList)
        {

            IList<Tuple<IToolParameter, double>> listOfScaledTuple = new List<Tuple<IToolParameter, double>>();

            foreach(IToolParameter currentToolParameter in toolParameterContainer.ToolParameter){
         
              Tuple<IToolParameter, double> currentLocalRange =  localRangeList.Where( x => x.Item1 == currentToolParameter).FirstOrDefault();
    
                //todo min - max

            // min
            // max cluster - _featureInCenter value divided by local range
            int oidColumnIndex = GetOIDColumnIndex();

                //max value of the tool parameter for the cluster
            IList<double> data = _dataTable.AsEnumerable().Where(x => _clusterIDs.Contains(x.Field<FieldTypeOID>(oidColumnIndex).OID)).Select(x => x.Field<double>(currentToolParameter.ColumnName)).Distinct().ToList();

            double max = data.Max();

            double actualValue = _dataTable.AsEnumerable().Where(x => x.Field<FieldTypeOID>(oidColumnIndex).OID == _featureID).Select(x => x.Field<double>(currentToolParameter.ColumnName)).FirstOrDefault();

                double result = (max - actualValue) / currentLocalRange.Item2;

                listOfScaledTuple.Add(Tuple.Create(currentToolParameter, result));
            }
            return listOfScaledTuple;
        }

        private IList<Tuple<IToolParameter, double>> LocalWeights(ToolParameterContainer toolParameterContainer, IList<Tuple<IToolParameter, double>> localRangeList, IList<Tuple<IToolParameter, double>> globalRangeList)
        {
            IList<Tuple<IToolParameter, double>> listOfLocalWeigtTupel = new List<Tuple<IToolParameter, double>>();

            double dividend, divisor = 0;

            foreach (IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
            {
                double localRange = localRangeList.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;
                double globalRange = globalRangeList.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;

                dividend = (currentToolParameter.ScaledWeight * localRange) / globalRange;

                foreach (IToolParameter currentToolParameter2 in toolParameterContainer.ToolParameter)
                {
                    double localRange2 = localRangeList.Where(x => x.Item1 == currentToolParameter2).FirstOrDefault().Item2;
                    double globalRange2 = globalRangeList.Where(x => x.Item1 == currentToolParameter2).FirstOrDefault().Item2;

                    divisor += (currentToolParameter2.ScaledWeight * localRange2) / globalRange2;
                }

                double result =  divisor == 0 ? 0 : dividend / divisor;

                listOfLocalWeigtTupel.Add(new Tuple<IToolParameter, double>(currentToolParameter,result));

            }

            return listOfLocalWeigtTupel;
        }
        
        // oid/fid, r1,r2, va1, va2, w1, w2, result
        public DataRow CalculateLWCL(DataRow row)
        {
            IList<Tuple<IToolParameter, double>> globalRange = GlobalRange(_toolParameterContainer);
            IList<Tuple<IToolParameter, double>> localRange = LocalRange(_clusterIDs, _toolParameterContainer);

            IList<Tuple<IToolParameter, double>> scaledValues = Scale(_toolParameterContainer, localRange);
            IList<Tuple<IToolParameter, double>> weights = LocalWeights(_toolParameterContainer, localRange, globalRange);

            row[0] = new FieldTypeOID(){ OID = _featureID};

            int index = 1;
            double result = 0;

            foreach (IToolParameter currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                double scaledValue = scaledValues.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;
                double weight = weights.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;

                row[index] = localRange.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;
                row[index + 1] = scaledValue;
                row[index + 2] = weight;

                result += scaledValue * weight;

                index += 3;
            }

            row[row.ItemArray.Length-1] = result;

            return row;
        }
    }
}
