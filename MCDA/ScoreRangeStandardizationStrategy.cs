using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    internal sealed class ScoreRangeStandardizationStrategy : IStandardizationStrategy
    {
        public double? Transform(IList<double> data, double actualValue, bool benefitCriterion = false)
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
            double maxValue = (double)column.Table.Compute("max(" + column.ColumnName + ")", String.Empty);
            double minValue = (double)column.Table.Compute("min(" + column.ColumnName + ")", String.Empty);

            double range = maxValue - minValue;

            int columnIndex = column.Ordinal;

            foreach (DataRow currentDataRow in column.Table.Rows)
            {
                if (benefitCriterion)
                {
                    if (range != 0)
                    {
                        currentDataRow[columnIndex] = ((double)currentDataRow.ItemArray[columnIndex] - minValue) / range;
                    }
                }
                else
                {
                    if (range != 0)
                    {
                        currentDataRow[columnIndex] = (maxValue - (double) currentDataRow.ItemArray[columnIndex]) / range;
                    }
                }
            }
        }
    }
}
