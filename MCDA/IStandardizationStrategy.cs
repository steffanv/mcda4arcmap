using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    /// <summary>
    /// Represents a strategy to standardize a list of numerical values.
    /// </summary>
    public interface IStandardizationStrategy
    {
        /// <summary>
        /// Transforms a DataColumn in place.
        /// </summary>
        /// <param name="column"></param>
        /// <param name="benefitCriterion"></param>
        void Transform(DataColumn column, bool benefitCriterion = true);

        /// <summary>
        /// Transforms a list in place.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="actualValue"></param>
        /// <param name="benefitCriterion"></param>
        /// <returns></returns>
        double? Transform(IEnumerable<double> data, double actualValue, bool benefitCriterion = false);
    }
}
