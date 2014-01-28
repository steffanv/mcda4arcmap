using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.Data;

namespace MCDA.Model
{
    internal sealed class MaximumScoreStandardizationStrategy : IStandardizationStrategy
    {
        public double? Transform(IList<double> data, double actualValue, bool benefitCriterion = false)
        {
            double maxValue = data.Max();

            if (maxValue == 0)
                return null;

            if (benefitCriterion)
                return actualValue / maxValue;
            else
                return 1d - (actualValue / maxValue);
        }

        public void Transform(DataColumn column, bool benefitCriterion = true)
        {
            // check other types and empty column
            double maxValue = (double)column.Table.Compute("max(" + column.ColumnName + ")", String.Empty);

            int columnIndex = column.Ordinal;

            foreach (DataRow currentDataRow in column.Table.Rows)
            {
                if (benefitCriterion)
                {
                    if (maxValue != 0)
                    {
                        currentDataRow[columnIndex] = (double)currentDataRow.ItemArray[columnIndex] / maxValue;
                    }
                }
                else
                {
                    currentDataRow[columnIndex] = 1d - ((double)currentDataRow.ItemArray[columnIndex] / maxValue);
                }
            }
        }
    }
}
