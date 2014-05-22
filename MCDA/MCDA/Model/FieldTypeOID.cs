using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    /// <summary>
    /// Use this class as custom type for data table column used for oids. 
    /// </summary>
   internal sealed class FieldTypeOID : IComparable<FieldTypeOID>
    {
        public int OID { get; set; }

        public override string ToString()
        {
            return OID.ToString();
        }

        public int CompareTo(FieldTypeOID fieldTypeOID)
        {
            return fieldTypeOID.OID.CompareTo(OID);
        }
    }
}
