using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
   public abstract class AbstractToolTemplate : ITool
    {
       protected abstract void PerformPreConditions();
        protected abstract void PerformAlgorithm();
        protected abstract void PerformScaling();

        public void Run()
        {
            PerformPreConditions();
            PerformScaling();
            PerformAlgorithm();
           
        }

        public abstract string DefaultResultColumnName {get; set;}
    }
}
