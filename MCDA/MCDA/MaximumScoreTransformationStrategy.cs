using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.Data;
using MCDA.Entity;

namespace MCDA.Model
{
    class MaximumScoreTransformationStrategy : ITransformationStrategy
    {

        public void Transform(DataColumn column, bool benefitCriterion = true)
        {
                double maxValue = (double)column.Table.Compute("max("+column.ColumnName+")",String.Empty);
                double minValue = (double)column.Table.Compute("min("+column.ColumnName+")",String.Empty);

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
                        currentDataRow[columnIndex] = 1d - ( (double) currentDataRow.ItemArray[columnIndex] / maxValue);
                    }
                }
        }
    }
}
