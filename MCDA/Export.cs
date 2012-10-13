using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace MCDA.Model
{
    sealed class Export
    {

        private Export() { }

        public static void ToCSV<T>(DataTable table, IList<T> toolParameter, string path) where T : IToolParameter
        {
            StringBuilder sb = new StringBuilder();

            if (toolParameter.Count > 0)
            {
                //"headline"
                sb.AppendLine(string.Join(",", Util.GetPropertyName(() => toolParameter[0].ColumnName), Util.GetPropertyName(() => toolParameter[0].Weight), Util.GetPropertyName(() => toolParameter[0].ScaledWeight), Util.GetPropertyName(() => toolParameter[0].IsBenefitCriterion)));

                foreach (IToolParameter currentToolParameter in toolParameter)
                {
                    sb.AppendLine(string.Join(",", currentToolParameter.ColumnName, currentToolParameter.Weight, currentToolParameter.ScaledWeight , currentToolParameter.IsBenefitCriterion));
                }

                sb.AppendLine();
            }

            IEnumerable<string> columnNames = table.Columns.Cast<DataColumn>().
                                              Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in table.Rows)
            {
                IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}
