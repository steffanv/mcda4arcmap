using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using MCDA.Extensions;

namespace MCDA.Model
{
   internal abstract class AbstractToolTemplate : ITool
    {  
        protected abstract void PerformAlgorithm();
        protected abstract void PerformScaling();

        public void Run()
        {
            PerformScaling();
            PerformAlgorithm();
           
        }

        protected DataTable PerformAlgorithmInParallel(DataTable dataTable, Action<DataTable> mcdaAlgorithm, int chunkSize = 1000)
        {
            IList<DataTable> tables = new List<DataTable>();

            IEnumerable<IEnumerable<DataRow>> chunks = dataTable.AsEnumerable().Partition(chunkSize);

            foreach (var chunk in chunks)
            {
                DataTable tempDataTable = dataTable.Clone();

                chunk.CopyToDataTable(tempDataTable, LoadOption.OverwriteChanges);

                tables.Add(tempDataTable);
            }

            Parallel.ForEach(tables, new ParallelOptions { MaxDegreeOfParallelism = 4 }, mcdaAlgorithm);

            DataTable finalTable = dataTable.Clone();

            foreach (var table in tables)
            {
                finalTable.Merge(table);    
            }

            return finalTable;
        }

        public abstract string DefaultResultColumnName {get; set;}
        public abstract DataTable Data{ get; }
        public abstract ToolParameterContainer ToolParameterContainer {get; set;}
        public abstract NormalizationStrategy TransformationStrategy { get; set; }
    }
}
