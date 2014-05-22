using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    internal static class NormalizationStrategyFactory
    {
        public static INormalizationStrategy GetStrategy(NormalizationStrategy transformationStrategy){

            switch (transformationStrategy)
	        {
		    case NormalizationStrategy.MaximumScoreNormalizationStrategy:
                    return new MaximumScoreNormalizationStrategy();
                
            case NormalizationStrategy.ScoreRangeNormalizationStrategy:
                    return new ScoreRangeNormalizationStrategy();
                
            case NormalizationStrategy.IdentityNormalizationStrategy:
                    return new IdentityNormalizationStrategy();
                
            case NormalizationStrategy.DefaultNormalizationStrategy:
                    return new MaximumScoreNormalizationStrategy();
                
            default:
                    return new MaximumScoreNormalizationStrategy();
                
	        }
        }
    }

    public enum NormalizationStrategy{

        MaximumScoreNormalizationStrategy,
        ScoreRangeNormalizationStrategy,
        IdentityNormalizationStrategy,
        DefaultNormalizationStrategy

    }
}
