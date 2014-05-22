using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using MCDA.Extensions;

namespace MCDA.Model
{
    internal sealed class ScoreRangeNormalizationStrategy : INormalizationStrategy
    {
        public double? Transform(IEnumerable<double> data, double actualValue, bool benefitCriterion = false)
        {
            double localRange = data.Max() - data.Min();

            if (localRange == 0)
                return null;

            if (benefitCriterion)
                return (actualValue - data.Min()) / localRange;
            else
                return (data.Max() - actualValue) / localRange;
        }

        public void Transform(DataColumn column, bool benefitCriterion = true)
        {
            if (!column.IsNumeric() || column.Table.Rows.Count <= 0)
                return;

            dynamic maxValue = column.Table.Compute("max(" + column.ColumnName + ")", String.Empty);
            dynamic minValue = column.Table.Compute("min(" + column.ColumnName + ")", String.Empty);

            dynamic range = maxValue - minValue;
            
            if (range == 0)
                return;
            
            int columnIndex = column.Ordinal;

            foreach (DataRow currentDataRow in column.Table.Rows)
            {
                if (benefitCriterion)
                {
                    dynamic currentValue = currentDataRow.ItemArray[columnIndex];
                    currentDataRow[columnIndex] = (currentValue - minValue) / range;
                }
                else
                {
                    dynamic currentValue = currentDataRow.ItemArray[columnIndex];
                    currentDataRow[columnIndex] = (maxValue - currentValue) / range;
                }
            }
        }
    }
}
