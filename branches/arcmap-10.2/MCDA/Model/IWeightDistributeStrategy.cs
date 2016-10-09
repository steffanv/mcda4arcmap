using System.Collections.Generic;

namespace MCDA.Model
{
    internal interface IWeightDistributionStrategy
    {
        void Distribute<T>(IList<T> listOfToolParameter) where T : class, IToolParameter;
    }
}
