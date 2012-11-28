using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Entity;
using MCDA.Extensions;

namespace MCDA.Model
{
    class ProportionalDistributionStrategy : IWeightDistributionStrategy
    {
        public void Distribute<T>(IList<T> listOfToolParameter) where T : class, IToolParameter
        {
            if (listOfToolParameter.Count == 0)
                return;

            IToolParameter lastWeightChangedToolParameter =  lastWeightChangedToolParameter = listOfToolParameter[0].LastWeightChangedToolParameter;

            double sumOfAllWeights = listOfToolParameter.Sum(t =>t.Weight);
          
            //we have to rescale
            if (sumOfAllWeights > 100)
            {
                double overrun = sumOfAllWeights - 100;  
                //how much do we have without the locked and the last changed?
                double availableSpace = listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter).Sum(t => t.Weight);

                //we have enough by taking from the non locked
                if (availableSpace >= overrun)
                {
                    //lets remove proportional
                    double sumOfChangeableWeights = listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter).Sum(t => t.Weight);

                    //in case we have only one element that is suitable we can directly remove all from this one
                    if (listOfToolParameter.Where(t => t.IsLocked == false && t.Weight > 0 && t != lastWeightChangedToolParameter).Count() == 1)
                    {
                        listOfToolParameter.Where(t => t.IsLocked == false && t.Weight > 0 && t != lastWeightChangedToolParameter).ForEach(t => t.Weight = (t.Weight - overrun));
                        return;
                    }
                    listOfToolParameter.Where(t => t.IsLocked == false && t.Weight > 0 && t != lastWeightChangedToolParameter).ForEach(t => t.Weight = (t.Weight - (t.Weight / sumOfChangeableWeights) * overrun));
                }

                //we have to resize also the latest change, but we try to keep as much as possible of the latest change
                else
                {
                    //lets set them to zero
                    listOfToolParameter.Where(t => t.IsLocked == false && t != lastWeightChangedToolParameter).ForEach(t => t.Weight = 0);

                    //how much are we still over?
                    double stillOver = listOfToolParameter.Sum(t => t.Weight) - 100;
                   
                    //and cut from the last changed
                    listOfToolParameter.Where(t => t == lastWeightChangedToolParameter).ForEach(t => t.Weight = (t.Weight - stillOver));
                }

                //sometimes the calculations produce values < 0 , thats often the case when they are already close to 0
                //one problem is that the UI shows still show 0, because thats the lower bound of the slider control
                listOfToolParameter.Where(t => t.Weight < 0).ForEach(t => t.Weight = 0);   
            }

        }
    }
}
