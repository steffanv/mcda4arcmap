using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace MCDA.Model
{
    public interface ITransformationStrategy
    {
        void Transform(DataColumn column, bool benefitCriterion = true);
    }
}
