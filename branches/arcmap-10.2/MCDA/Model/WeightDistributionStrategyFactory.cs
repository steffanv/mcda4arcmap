namespace MCDA.Model
{
    internal static class WeightDistributionStrategyFactory
    {
        private static IWeightDistributionStrategy NewProportionalDistributionStrategy()
        {
            return new ProportionalDistributionStrategy();
        }

        public static IWeightDistributionStrategy DefaultWeightDistributionStrategy()
        {
            return NewProportionalDistributionStrategy();
        }
    }
}
