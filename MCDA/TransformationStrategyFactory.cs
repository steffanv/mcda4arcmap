using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    public static class TransformationStrategyFactory
    {
        public static ITransformationStrategy GetStrategy(TransformationStrategy transformationStrategy){

            if(transformationStrategy == TransformationStrategy.MaximumScoreTransformationStrategy)
                return new MaximumScoreTransformationStrategy();
            if(transformationStrategy == TransformationStrategy.ScoreRangeTransformationStrategy)
                return new ScoreRangeTransformationStrategy();
            if(transformationStrategy == TransformationStrategy.DefaultTransformationStrategy)
                return new MaximumScoreTransformationStrategy();

            return new MaximumScoreTransformationStrategy();
        }
    }

    public enum TransformationStrategy{

        MaximumScoreTransformationStrategy,
        ScoreRangeTransformationStrategy,
        DefaultTransformationStrategy

    }
}
