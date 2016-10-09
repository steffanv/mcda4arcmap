using System.Collections.Generic;

namespace MCDA.Model
{
    /// <summary>
    /// Does not change the input data in any kind.
    /// </summary>
    internal sealed class IdentityNormalizationStrategy : INormalizationStrategy
    {
        public void Transform(System.Data.DataColumn column, bool benefitCriterion = true)
        {
            
        }

        public double? Transform(IEnumerable<double> data, double actualValue, bool benefitCriterion = false)
        {
            return actualValue;
        }
    }
}
