using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    class ScoreRangeTransformationStrategy : ITransformationStrategy
    {
        public void Transform(DataColumn column, bool benefitCriterion = true)
        {
            double maxValue = (double)column.Table.Compute("max(" + column.ColumnName + ")", String.Empty);
            double minValue = (double)column.Table.Compute("min(" + column.ColumnName + ")", String.Empty);
            double maxMinDifference = maxValue - minValue;

            int columnIndex = column.Ordinal;

            foreach (DataRow currentDataRow in column.Table.Rows)
            {
                if (benefitCriterion)
                {
                    if (maxMinDifference != 0)
                    {
                        currentDataRow[columnIndex] = ((double)currentDataRow.ItemArray[columnIndex] - minValue) / maxMinDifference;
                    }
                }
                else
                {
                    if (maxMinDifference != 0)
                    {
                        currentDataRow[columnIndex] = (maxValue - (double) currentDataRow.ItemArray[columnIndex]) / maxMinDifference;
                    }
                }
            }
        }
    }
}
