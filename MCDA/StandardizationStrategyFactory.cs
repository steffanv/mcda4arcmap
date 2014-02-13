using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCDA.Model
{
    internal static class StandardizationStrategyFactory
    {
        public static IStandardizationStrategy GetStrategy(StandardizationStrategy transformationStrategy){

            switch (transformationStrategy)
	        {
		    case StandardizationStrategy.MaximumScoreStandardizationStrategy:
                    return new MaximumScoreStandardizationStrategy();
                
            case StandardizationStrategy.ScoreRangeStandardizationStrategy:
                    return new ScoreRangeStandardizationStrategy();
                
            case StandardizationStrategy.IdentityStandardizationStrategy:
                    return new IdentityStandardizationStrategy();
                
            case StandardizationStrategy.DefaultStandardizationStrategy:
                    return new MaximumScoreStandardizationStrategy();
                
            default:
                    return new MaximumScoreStandardizationStrategy();
                
	        }
        }
    }

    public enum StandardizationStrategy{

        MaximumScoreStandardizationStrategy,
        ScoreRangeStandardizationStrategy,
        IdentityStandardizationStrategy,
        DefaultStandardizationStrategy

    }
}
