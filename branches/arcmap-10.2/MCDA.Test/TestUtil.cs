using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCDA.Test
{
    internal static class TestUtil
    {
        public static DataColumn CreateDataColumn<T>(IEnumerable<T> data)
        {

            DataTable dt = new DataTable();
            DataColumn dc = new DataColumn("Column", typeof(T));
            dt.Columns.Add(dc);

            foreach (var currentValue in data)
            {
                DataRow dr = dt.NewRow();
                dr[0] = currentValue;
                dt.Rows.Add(dr);
            }

            return dt.Columns[0];
        }
    }
}
