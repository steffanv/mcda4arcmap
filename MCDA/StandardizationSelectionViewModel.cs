using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using System.ComponentModel;
using MCDA.Extensions;

namespace MCDA.ViewModel
{
    class StandardizationSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
   
        private TransformationStrategy _selectedTtransformationStrategy;

        public bool IsScoreRangeProcedure
        {
            get { return _selectedTtransformationStrategy == TransformationStrategy.ScoreRangeTransformationStrategy; }
            set { _selectedTtransformationStrategy = TransformationStrategy.ScoreRangeTransformationStrategy;

            PropertyChanged.Notify(() => IsMaximumScoreRangeProcedure);
            PropertyChanged.Notify(() => IsScoreRangeProcedure);
            }
            
        }
        public bool IsMaximumScoreRangeProcedure
        {
            get { return _selectedTtransformationStrategy == TransformationStrategy.MaximumScoreTransformationStrategy; }
            set { _selectedTtransformationStrategy = TransformationStrategy.MaximumScoreTransformationStrategy;

            PropertyChanged.Notify(() => IsMaximumScoreRangeProcedure);
            PropertyChanged.Notify(() => IsScoreRangeProcedure);

            }
        }

        public TransformationStrategy SelectedTransformationStrategy{

            get{ return _selectedTtransformationStrategy; }

            set { _selectedTtransformationStrategy = value;

            PropertyChanged.Notify(() => IsMaximumScoreRangeProcedure);
            PropertyChanged.Notify(() => IsScoreRangeProcedure);

            }
        }
    }
}
