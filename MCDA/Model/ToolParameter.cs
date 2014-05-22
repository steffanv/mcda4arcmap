using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;

namespace MCDA.Model
{

   internal sealed class ToolParameter : AbstractToolParameter
   {
        
        private static IToolParameter _lastWeightChangedToolParameter;

        public ToolParameter(string columnName)
        {
            _columnName = columnName;
        }

        public override IToolParameter LastWeightChangedToolParameter
        {
            get { return _lastWeightChangedToolParameter;}
            set { _lastWeightChangedToolParameter = value; }
        }

        public override double Weight
        {
            get { return _weight; }
            set { _lastWeightChangedToolParameter = this;

                if(value < 0)
                    _weight = 0;
                else if (value > 100)
                    _weight = 100;
                else
                    _weight = value;

            OnPropertyChanged(new PropertyChangedEventArgs("Weight"));
            }
        }

        public override IToolParameter DeepClone()
        {
            ToolParameter copy = new ToolParameter(_columnName);

            copy._isBenefitCriterion = _isBenefitCriterion;
            copy._isLocked = _isLocked;
            copy._isOID = _isOID;
            copy._weight = _weight;

            return copy;
        }
   }
}
