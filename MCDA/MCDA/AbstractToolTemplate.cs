using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

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

        public abstract string DefaultResultColumnName {get; set;}
        public abstract DataTable Data{ get; }
        public abstract ToolParameterContainer ToolParameterContainer {get; set;}
        public abstract StandardizationStrategy TransformationStrategy { get; set; }
    }
}
