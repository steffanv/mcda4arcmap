using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    internal interface IWeightDistributionStrategy
    {
        void Distribute<T>(IList<T> listOfToolParameter) where T : class, IToolParameter;
    }
}
