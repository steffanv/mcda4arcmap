using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;
using MCDA.Model;

namespace MCDA.Misc
{
    internal static class Export
    {
        public static void ToCSV<T>(DataTable table, IList<T> toolParameter, string path, params Tuple<string,object>[] param) where T : IToolParameter
        {
            var sb = new StringBuilder();

            if (toolParameter.Count > 0)
            {
                //"headline"
                sb.AppendLine(string.Join(",", Util.GetPropertyName(() => toolParameter.First().ColumnName), Util.GetPropertyName(() => toolParameter.First().Weight), Util.GetPropertyName(() => toolParameter.First().ScaledWeight), Util.GetPropertyName(() => toolParameter.First().IsBenefitCriterion)));

                foreach (var currentToolParameter in toolParameter)
                {
                    sb.AppendLine(string.Join(",", currentToolParameter.ColumnName, currentToolParameter.Weight, currentToolParameter.ScaledWeight , currentToolParameter.IsBenefitCriterion));
                }

                sb.AppendLine();
            }

            
            sb.AppendLine(string.Join(",", param.Select(t => t.Item1)));
            sb.AppendLine(string.Join(",",param.Select(t => t.Item2.ToString())));

            sb.AppendLine();

            var columnNames = table.Columns.Cast<DataColumn>().Select(column => column.ColumnName);
            sb.AppendLine(string.Join(",", columnNames));

            foreach (DataRow row in table.Rows)
            {
                var fields = row.ItemArray.Select(field => field.ToString());
                sb.AppendLine(string.Join(",", fields));
            }

            File.WriteAllText(path, sb.ToString());
        }
    }
}
