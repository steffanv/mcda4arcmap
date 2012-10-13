using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    public static class TransformationStrategyFactory
    {
        public static ITransformationStrategy NewMaximumScoreTransformationStrategy()
        {
            return new MaximumScoreTransformationStrategy();
        }

        public static ITransformationStrategy NewScoreRangeTransformationStrategy()
        {
            return new ScoreRangeTransformationStrategy();
        }

        public static ITransformationStrategy DefaultTransformationStrategy()
        {
            return NewMaximumScoreTransformationStrategy();
        }
    }
}
