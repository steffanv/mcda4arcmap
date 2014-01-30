using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCDA.Extensions
{
    internal static class DataColumnExtension
    {
        public static bool IsNumeric(this DataColumn column)
        {
            if (column == null)
                return false;
           
            var numericTypes = new[] { typeof(Byte), typeof(Decimal), typeof(Double), typeof(Int16), typeof(Int32), typeof(Int64), typeof(SByte),
                                typeof(Single), typeof(UInt16), typeof(UInt32), typeof(UInt64)};

            return numericTypes.Contains(column.DataType);
        }
    }
}
