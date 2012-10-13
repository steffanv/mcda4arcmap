using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
   public abstract class AbstractToolTemplate : ITool
    {
        protected abstract void PerformAlgorithm();
        protected abstract void PerformScaling();

        public void Run()
        {
            PerformScaling();
            PerformAlgorithm();
           
        }

        public abstract string DefaultResultColumnName
        {
            get;
        }
    }
}
