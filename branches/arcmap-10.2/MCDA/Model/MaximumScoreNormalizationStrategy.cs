using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using MCDA.Extensions;

namespace MCDA.Model
{
    internal sealed class MaximumScoreNormalizationStrategy : INormalizationStrategy
    {
        public double? Transform(IEnumerable<double> data, double actualValue, bool benefitCriterion = false)
        {
            var maxValue = data.Max();

            if (maxValue == 0)
            {
                return null;
            }

            if (benefitCriterion)
            {
                return actualValue/maxValue;
            }
            else
            {
                return 1d - (actualValue/maxValue);
            }
        }

        public void Transform(DataColumn column, bool benefitCriterion = true)
        {
            if (!column.IsNumeric() || column.Table.Rows.Count <= 0)
            {
                return;
            }

            dynamic maxValue = column.Table.Compute("max(" + column.ColumnName + ")", String.Empty);

            if (maxValue == 0)
            {
                return;
            }

            var columnIndex = column.Ordinal;  

            foreach (DataRow currentDataRow in column.Table.Rows)
            {
                if (benefitCriterion)
                {
                    dynamic currentValue = currentDataRow.ItemArray[columnIndex];
                    currentDataRow[columnIndex] = currentValue / maxValue;
                }

                else
                {
                    dynamic currentValue = currentDataRow.ItemArray[columnIndex];
                    currentDataRow[columnIndex] = 1d - (currentValue / maxValue);
                }
            }
        }
    }
}
