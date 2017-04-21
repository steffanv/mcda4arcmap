using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using MCDA.Extensions;
using MCDA.Misc;

namespace MCDA.Model
{
   internal abstract class AbstractToolTemplate : ITool
    {  
        protected abstract void PerformAlgorithm(ProgressHandler handler = null);
        protected abstract void PerformScaling();

        public void Run(ProgressHandler progressHandler = null)
        {
            //progressHandler.ProvideTask()
            progressHandler?.OnProgress(10, "Perform scaling");
            PerformScaling();
            progressHandler?.OnProgress(10, "Perform algorithm");
            PerformAlgorithm(progressHandler);
            progressHandler?.OnProgress(100, "ende");
        }

        protected static DataTable PerformAlgorithmInParallel(DataTable dataTable, Action<DataTable> mcdaAlgorithm, int chunkSize = 1000)
        {
            IList<DataTable> tables = new List<DataTable>();

            var chunks = dataTable.AsEnumerable().Partition(chunkSize);

            foreach (var chunk in chunks)
            {
                var tempDataTable = dataTable.Clone();

                chunk.CopyToDataTable(tempDataTable, LoadOption.OverwriteChanges);

                tables.Add(tempDataTable);
            }

            Parallel.ForEach(tables, new ParallelOptions { MaxDegreeOfParallelism = 4 }, mcdaAlgorithm);

            var finalTable = dataTable.Clone();

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
