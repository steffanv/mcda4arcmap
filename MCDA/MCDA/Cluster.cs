using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MCDA.Extensions;

namespace MCDA.Model
{
    class Cluster
    {
        private ToolParameterContainer _toolParameterContainer;
        private DataTable _dataTable;
        private int _featureID;
        private IList<int> _clusterIDs;
        private TransformationStrategy _transformationStrategy;

        private IList<Tuple<IToolParameter, double>> _globalRange;
        private IList<Tuple<IToolParameter, double>> _localRange;
        private IList<Tuple<IToolParameter, double?>> _scaledValues;
        private IList<Tuple<IToolParameter, double?>> _weights;

        public Cluster(int featureID, IList<int> clusterIDs, DataTable dt, ToolParameterContainer toolParameterContainer, TransformationStrategy transformationStrategy)
        {
            _featureID = featureID;
            _clusterIDs = clusterIDs;
            _dataTable = dt;
            _toolParameterContainer = toolParameterContainer;
            _transformationStrategy = transformationStrategy;

            //the feature id is also part of the cluster!
            _clusterIDs.Add(featureID);
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
                IList<double> data = _dataTable.AsEnumerable().Select(x => x.Field<double>(currentToolParameter.ColumnName)).ToList();

                listOfRangeTuple.Add(Tuple.Create(currentToolParameter, data.Max() - data.Min()));
            }

            return listOfRangeTuple;
        }

        private IList<Tuple<IToolParameter, double>> LocalRange(IList<int> clusterIDs, ToolParameterContainer toolParameterContainer)
        {
            int oidColumnIndex = GetOIDColumnIndex();

            IList<Tuple<IToolParameter, double>> listOfRangeTuple = new List<Tuple<IToolParameter, double>>();

            foreach (IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
            {
                IList<double> data = _dataTable.AsEnumerable().Where(x => _clusterIDs.Contains(x.Field<FieldTypeOID>(oidColumnIndex).OID)).Select(x => x.Field<double>(currentToolParameter.ColumnName)).ToList();

                listOfRangeTuple.Add(Tuple.Create(currentToolParameter, data.Max() - data.Min()));
            }

            return listOfRangeTuple;
        }

        private IList<Tuple<IToolParameter, double?>> Scale(ToolParameterContainer toolParameterContainer, IList<Tuple<IToolParameter, double>> localRangeList)
        {
            IList<Tuple<IToolParameter, double?>> listOfScaledTuple = new List<Tuple<IToolParameter, double?>>();

            foreach (IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
            {
                Tuple<IToolParameter, double> currentLocalRange = localRangeList.Where(x => x.Item1 == currentToolParameter).FirstOrDefault();

                int oidColumnIndex = GetOIDColumnIndex();

                IList<double> data = _dataTable.AsEnumerable().Where(x => _clusterIDs.Contains(x.Field<FieldTypeOID>(oidColumnIndex).OID)).Select(x => x.Field<double>(currentToolParameter.ColumnName)).Distinct().ToList();

                double actualValue = _dataTable.AsEnumerable().Where(x => x.Field<FieldTypeOID>(oidColumnIndex).OID == _featureID).Select(x => x.Field<double>(currentToolParameter.ColumnName)).FirstOrDefault();

                double? result = TransformationStrategyFactory.GetStrategy(_transformationStrategy).Transform(data, actualValue, currentToolParameter.IsBenefitCriterion);

                listOfScaledTuple.Add(Tuple.Create(currentToolParameter, result));
            }

            return listOfScaledTuple;
        }

        private IList<Tuple<IToolParameter, double?>> LocalWeights(ToolParameterContainer toolParameterContainer, IList<Tuple<IToolParameter, double>> localRangeList, IList<Tuple<IToolParameter, double>> globalRangeList)
        {
            IList<Tuple<IToolParameter, double?>> listOfLocalWeightTupel = new List<Tuple<IToolParameter, double?>>();

            double? dividend, divisor = 0;

            foreach (IToolParameter currentToolParameter2 in toolParameterContainer.ToolParameter)
            {
                double localRange2 = localRangeList.Where(x => x.Item1 == currentToolParameter2).First().Item2;
                double globalRange2 = globalRangeList.Where(x => x.Item1 == currentToolParameter2).First().Item2;

                if (globalRange2 == 0)
                {
                    divisor = null;
                    break;
                }

                divisor = divisor + ((currentToolParameter2.ScaledWeight * localRange2) / globalRange2);
            }

            if (divisor == 0)
                divisor = null;

            foreach (IToolParameter currentToolParameter in toolParameterContainer.ToolParameter)
            {
                double localRange = localRangeList.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;
                double globalRange = globalRangeList.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;

                if (globalRange == 0)
                    dividend = null;
                else
                    dividend = (currentToolParameter.ScaledWeight * localRange) / globalRange;

                if(dividend.HasValue && divisor.HasValue)
                    listOfLocalWeightTupel.Add(new Tuple<IToolParameter, double?>(currentToolParameter, dividend / divisor));

                if(!dividend.HasValue || !divisor.HasValue)
                    listOfLocalWeightTupel.Add(new Tuple<IToolParameter, double?>(currentToolParameter, null));               
            }

            return listOfLocalWeightTupel;
        }

        public void Calculate()
        {
             _globalRange = GlobalRange(_toolParameterContainer);
             _localRange = LocalRange(_clusterIDs, _toolParameterContainer);

             _scaledValues = Scale(_toolParameterContainer, _localRange);
             _weights = LocalWeights(_toolParameterContainer, _localRange, _globalRange);
        }

        public bool IsResultNull()
        {
            Calculate();

            foreach (IToolParameter currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                double? scaledValue = _scaledValues.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;
                double? weight = _weights.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;

                if (!scaledValue.HasValue || !weight.HasValue)
                    return true;
            }

            return false;
        }
        
        // oid/fid, r1,s1, w1, r2, s2, w3, result
        public DataRow FillRowWithResults(DataRow row)
        {
            row[0] = new FieldTypeOID(){ OID = _featureID};

            StringBuilder sb = new StringBuilder();

            foreach(int currentID in _clusterIDs){

                if(sb.Length == 0)
                    sb.Append(currentID);
                else
                    sb.Append("; " + currentID);
            }

            row[1] = sb.ToString();

            int index = 2;
            double? result = 0;

            foreach (IToolParameter currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                double? scaledValue = _scaledValues.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;
                double? weight = _weights.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;

                row[index] = _localRange.Where(x => x.Item1 == currentToolParameter).FirstOrDefault().Item2;

                if (scaledValue.HasValue)
                    row[index + 1] = scaledValue;
                else
                    row[index + 1] = DBNull.Value;

                if (weight.HasValue)
                    row[index + 2] = weight;
                else
                    row[index + 2] = DBNull.Value;

                if (scaledValue.HasValue && weight.HasValue && result.HasValue)
                    result += scaledValue * weight;
                else
                    result = null;

                index += 3;
            }

            if(result.HasValue)
                row.SetField(row.ItemArray.Length - 1, Math.Round(result.Value < 0 ? 0 : result.Value, 6));
            else
                row.SetField(row.ItemArray.Length - 1, DBNull.Value);

            return row;
        }
    }
}
