using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;

namespace MCDA.Model
{
    class ProportionalDistributionStrategy : IWeightDistributionStrategy
    {
        public void Distribute<T>(IList<T> listOfToolParameter) where T : class, IToolParameter
        {
            if (listOfToolParameter.Count == 0)
                return;

            IToolParameter lastWeightChangedToolParameter =  lastWeightChangedToolParameter = listOfToolParameter.FirstOrDefault().LastWeightChangedToolParameter;

            double difference = 100 - listOfToolParameter.Sum(t => t.Weight);

            if (listOfToolParameter.Where(t => !t.IsLocked && t != lastWeightChangedToolParameter).Sum(t => t.Weight) == 0  && difference < 0)
            {
                listOfToolParameter.Where(t => t == lastWeightChangedToolParameter).FirstOrDefault().Weight += difference;
            }
            else
            {
                //assume values = 0 are only close to zero (0.001) otherwise we could never increase the value while distributing the weight
                double hundredPercentValueOfWeights = listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter).Sum(t => t.Weight);
                listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter && t.Weight == 0).ForEach(t => hundredPercentValueOfWeights += 0.001);

                foreach (IToolParameter currentToolParameter in listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter).ToList())
                {

                    double weight = currentToolParameter.Weight;

                    currentToolParameter.Weight = weight + ((weight == 0 ? 0.001 : weight) / hundredPercentValueOfWeights) * difference;
                }
            }

            listOfToolParameter.Where(t => t.Weight < 0).ForEach(t => t.Weight = 0);

            listOfToolParameter.Where(t => t.Weight > 100).ForEach(t => t.Weight = 100);  

        }
    }
}
