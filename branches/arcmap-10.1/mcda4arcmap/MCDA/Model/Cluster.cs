using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    internal sealed class Cluster
    {
        private readonly ToolParameterContainer _toolParameterContainer;
        private readonly DataTable _dataTable;
        private readonly int _featureId;
        private readonly IList<int> _clusterIDs;
        private readonly NormalizationStrategy _transformationStrategy;

        private IList<Tuple<IToolParameter, double>> _globalRange;
        private IList<Tuple<IToolParameter, double>> _localRange;
        private IList<Tuple<IToolParameter, double?>> _scaledValues;
        private IList<Tuple<IToolParameter, double?>> _weights;

        public Cluster(int featureId, IList<int> clusterIDs, DataTable dt, ToolParameterContainer toolParameterContainer, NormalizationStrategy transformationStrategy)
        {
            _featureId = featureId;
            _clusterIDs = clusterIDs;
            _dataTable = dt;
            _toolParameterContainer = toolParameterContainer;
            _transformationStrategy = transformationStrategy;

            //the feature id is also part of the cluster!
            _clusterIDs.Add(featureId);
        }

        private int GetOIDColumnIndex()
        {
            foreach (DataColumn currentDataColumn in _dataTable.Columns)
            {
                if (currentDataColumn.DataType == typeof(FieldTypeOID))
                {
                    return currentDataColumn.Ordinal;
                }
            }

            return -1;
        }

        private IList<Tuple<IToolParameter, double>> GlobalRange(ToolParameterContainer toolParameterContainer)
        {
            IList<Tuple<IToolParameter, double>> listOfRangeTuple = new List<Tuple<IToolParameter, double>>();

            foreach (var currentToolParameter in toolParameterContainer.ToolParameter)
            {
                var parameter = currentToolParameter;
                IList<double> data = _dataTable.AsEnumerable().Select(x => x.Field<double>(parameter.ColumnName)).ToList();

                listOfRangeTuple.Add(Tuple.Create(currentToolParameter, data.Max() - data.Min()));
            }

            return listOfRangeTuple;
        }

        private IList<Tuple<IToolParameter, double>> LocalRange(IList<int> clusterIDs, ToolParameterContainer toolParameterContainer)
        {
            int oidColumnIndex = GetOIDColumnIndex();

            IList<Tuple<IToolParameter, double>> listOfRangeTuple = new List<Tuple<IToolParameter, double>>();

            foreach (var currentToolParameter in toolParameterContainer.ToolParameter)
            {
                var parameter = currentToolParameter;
                IList<double> data = _dataTable.AsEnumerable().Where(x => _clusterIDs.Contains(x.Field<FieldTypeOID>(oidColumnIndex).OID)).Select(x => x.Field<double>(parameter.ColumnName)).ToList();

                listOfRangeTuple.Add(Tuple.Create(currentToolParameter, data.Max() - data.Min()));
            }

            return listOfRangeTuple;
        }

        private IList<Tuple<IToolParameter, double?>> Scale(ToolParameterContainer toolParameterContainer, IList<Tuple<IToolParameter, double>> localRangeList)
        {
            IList<Tuple<IToolParameter, double?>> listOfScaledTuple = new List<Tuple<IToolParameter, double?>>();

            foreach (var currentToolParameter in toolParameterContainer.ToolParameter)
            {
                var currentLocalRange = localRangeList.FirstOrDefault(x => x.Item1 == currentToolParameter);

                var oidColumnIndex = GetOIDColumnIndex();

                var parameter = currentToolParameter;
                IList<double> data = _dataTable.AsEnumerable().Where(x => _clusterIDs.Contains(x.Field<FieldTypeOID>(oidColumnIndex).OID)).Select(x => x.Field<double>(parameter.ColumnName)).Distinct().ToList();

                var toolParameter = currentToolParameter;
                var actualValue = _dataTable.AsEnumerable().Where(x => x.Field<FieldTypeOID>(oidColumnIndex).OID == _featureId).Select(x => x.Field<double>(toolParameter.ColumnName)).FirstOrDefault();

                var result = NormalizationStrategyFactory.GetStrategy(_transformationStrategy).Transform(data, actualValue, currentToolParameter.IsBenefitCriterion);

                listOfScaledTuple.Add(Tuple.Create(currentToolParameter, result));
            }

            return listOfScaledTuple;
        }

        private static IList<Tuple<IToolParameter, double?>> LocalWeights(ToolParameterContainer toolParameterContainer, IList<Tuple<IToolParameter, double>> localRangeList, IList<Tuple<IToolParameter, double>> globalRangeList)
        {
            IList<Tuple<IToolParameter, double?>> listOfLocalWeightTupel = new List<Tuple<IToolParameter, double?>>();

            double? divisor = 0;

            foreach (var currentToolParameter2 in toolParameterContainer.ToolParameter)
            {
                var localRange2 = localRangeList.First(x => x.Item1 == currentToolParameter2).Item2;
                var globalRange2 = globalRangeList.First(x => x.Item1 == currentToolParameter2).Item2;

                if (globalRange2 == 0)
                {
                    divisor = null;
                    break;
                }

                divisor = divisor + ((currentToolParameter2.ScaledWeight * localRange2) / globalRange2);
            }

            if (divisor == 0)
            {
                divisor = null;
            }

            foreach (var currentToolParameter in toolParameterContainer.ToolParameter)
            {
                var localRange = localRangeList.FirstOrDefault(x => x.Item1 == currentToolParameter).Item2;
                var globalRange = globalRangeList.FirstOrDefault(x => x.Item1 == currentToolParameter).Item2;

                double? dividend;
                if (globalRange == 0)
                {
                    dividend = null;
                }
                else
                {
                    dividend = (currentToolParameter.ScaledWeight * localRange) / globalRange;
                }

                if (dividend.HasValue && divisor.HasValue)
                {
                    listOfLocalWeightTupel.Add(new Tuple<IToolParameter, double?>(currentToolParameter, dividend / divisor));
                }
                else
                {
                    listOfLocalWeightTupel.Add(new Tuple<IToolParameter, double?>(currentToolParameter, null));
                }
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
                var scaledValue = _scaledValues.FirstOrDefault(x => x.Item1 == currentToolParameter).Item2;
                var weight = _weights.FirstOrDefault(x => x.Item1 == currentToolParameter).Item2;

                if (!scaledValue.HasValue || !weight.HasValue)
                {
                    return true;
                }
            }
            return false;
        }
        
        // oid/fid, r1,s1, w1, r2, s2, w3, result
        public DataRow FillRowWithResults(DataRow row)
        {
            row[0] = new FieldTypeOID(){ OID = _featureId};

            var stringBuilder = new StringBuilder();

            foreach(int currentId in _clusterIDs){

                if (stringBuilder.Length == 0)
                {
                    stringBuilder.Append(currentId);
                }
                else
                {
                    stringBuilder.Append("; " + currentId);
                }
            }

            row[1] = stringBuilder.ToString();

            var index = 2;
            double? result = 0;

            foreach (var currentToolParameter in _toolParameterContainer.ToolParameter)
            {
                var scaledValue = _scaledValues.First(x => x.Item1 == currentToolParameter).Item2;
                var weight = _weights.First(x => x.Item1 == currentToolParameter).Item2;

                row[index] = _localRange.First(x => x.Item1 == currentToolParameter).Item2;

                if (scaledValue.HasValue)
                {
                    row[index + 1] = scaledValue;
                }
                else
                {
                    row[index + 1] = DBNull.Value;
                }

                if (weight.HasValue)
                {
                    row[index + 2] = weight;
                }
                else
                {
                    row[index + 2] = DBNull.Value;
                }

                if (scaledValue.HasValue && weight.HasValue && result.HasValue)
                {
                    result += scaledValue*weight;
                }
                else
                {
                    result = null;
                }

                index += 3;
            }

            if (result.HasValue)
            {
                row.SetField(row.ItemArray.Length - 1, Math.Round(result.Value < 0 ? 0 : result.Value, 6));
            }
            else
            {
                row.SetField(row.ItemArray.Length - 1, DBNull.Value);
            }

            return row;
        }
    }
}
