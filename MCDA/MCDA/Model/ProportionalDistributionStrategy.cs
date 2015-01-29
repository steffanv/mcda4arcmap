using System.Collections.Generic;
using System.Linq;

namespace MCDA.Model
{
    internal sealed class ProportionalDistributionStrategy : IWeightDistributionStrategy
    {
        public void Distribute<T>(IList<T> listOfToolParameter) where T : class, IToolParameter
        {
            if (listOfToolParameter.Count == 0)
            {
                return;
            }

            IToolParameter lastWeightChangedToolParameter = lastWeightChangedToolParameter = listOfToolParameter.First().LastWeightChangedToolParameter;

            var difference = 100 - listOfToolParameter.Sum(t => t.Weight);

            // all except one (last changed) are locked => we can not change anything
            if ((listOfToolParameter.Count(toolParameter => toolParameter.IsLocked) == listOfToolParameter.Count() - 1) &&
                (!lastWeightChangedToolParameter.IsLocked))
            {
                lastWeightChangedToolParameter.Weight += difference;
            }

            // this is also true if all are locked
            if (listOfToolParameter.Count(t => t.IsLocked) == listOfToolParameter.Count())
            {
                lastWeightChangedToolParameter.Weight += difference;
            }

            else if (listOfToolParameter.Where(t => !t.IsLocked && t != lastWeightChangedToolParameter).Sum(t => t.Weight) == 0 && difference < 0)
            {
                lastWeightChangedToolParameter.Weight += difference;
            }

            else
            {
                //assume values = 0 are only close to zero (0.01) otherwise we could never increase the value while distributing the weight
                var hundredPercentValueOfWeights =
                    listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter)
                        .Sum(t => t.Weight);

                foreach (
                    var currentToolParameter in
                        listOfToolParameter.Where(
                            t => t.IsLocked == false && t != lastWeightChangedToolParameter && t.Weight == 0))
                {
                    currentToolParameter.Weight = 0.01;
                }

                foreach (
                    var currentToolParameter in
                        listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter)
                            .ToList())
                {
                    var weight = currentToolParameter.Weight;

                    // this is the case were all non last changed values are zero thus we can simply add equally the difference
                    if (hundredPercentValueOfWeights == 0)
                    {
                        currentToolParameter.Weight = difference/listOfToolParameter.Count();
                    }
                    else
                    {
                        currentToolParameter.Weight = weight +
                                                      ((weight == 0 ? 0.01 : weight)/hundredPercentValueOfWeights)*
                                                      difference;
                    }
                }
            }

            foreach (var currentToolParamter in listOfToolParameter.Where(t => t.Weight <= 0.01))
            {
                currentToolParamter.Weight = 0;
            }

            foreach (var currentToolParameter in listOfToolParameter.Where(t => t.Weight > 100))
            {
                currentToolParameter.Weight = 100;
            }
        }
    }
}
