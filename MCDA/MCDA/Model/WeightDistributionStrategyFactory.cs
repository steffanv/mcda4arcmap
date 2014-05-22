using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    internal static class WeightDistributionStrategyFactory
    {

        public static IWeightDistributionStrategy NewProportionalDistributionStrategy()
        {
            return new ProportionalDistributionStrategy();
        }

        public static IWeightDistributionStrategy DefaultWeightDistributionStrategy()
        {
            return NewProportionalDistributionStrategy();
        }
    }
}
