using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    internal static class StandardizationStrategyFactory
    {
        public static IStandardizationStrategy GetStrategy(StandardizationStrategy transformationStrategy){

            if(transformationStrategy == StandardizationStrategy.MaximumScoreStandardizationStrategy)
                return new MaximumScoreStandardizationStrategy();
            if(transformationStrategy == StandardizationStrategy.ScoreRangeStandardizationStrategy)
                return new ScoreRangeStandardizationStrategy();
            if(transformationStrategy == StandardizationStrategy.DefaultStandardizationStrategy)
                return new MaximumScoreStandardizationStrategy();

            return new MaximumScoreStandardizationStrategy();
        }
    }

    public enum StandardizationStrategy{

        MaximumScoreStandardizationStrategy,
        ScoreRangeStandardizationStrategy,
        DefaultStandardizationStrategy

    }
}
