using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;

namespace MCDA.ViewModel
{
    class StandardizationSelectionViewModel
    {
        
        private bool _isScoreRangeProcedure;
        private bool _isMaximumScoreProcedure;

        public bool IsScoreRangeProcedure
        {
            get { return _isScoreRangeProcedure; }
            set { _isScoreRangeProcedure = value; }
        }
        public bool IsMaximumScoreRangeProcedure
        {
            get { return _isMaximumScoreProcedure; }
            set { _isMaximumScoreProcedure = value;  }
        }

        public ITransformationStrategy SelectedTransformationStrategy{

            get{
                if (_isMaximumScoreProcedure)
                    return TransformationStrategyFactory.NewMaximumScoreTransformationStrategy();
                if (_isScoreRangeProcedure)
                    return TransformationStrategyFactory.NewScoreRangeTransformationStrategy();

                return TransformationStrategyFactory.DefaultTransformationStrategy();
            }
        }
    }
}
