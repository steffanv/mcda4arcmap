using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;

namespace MCDA.Model
{

   public class WLCToolParameter : AbstractToolParameter
   {
        
        private static IToolParameter _lastWeightChangedToolParameter;

        private static bool _isPropertiesLocked;

        public WLCToolParameter(string columnName)
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
            _weight = value;
            OnPropertyChanged(new PropertyChangedEventArgs("Weight"));
            }
        }

        public override IToolParameter DeepClone()
        {
            WLCToolParameter copy = new WLCToolParameter(_columnName);

            copy._isBenefitCriterion = _isBenefitCriterion;
            copy._isLocked = _isLocked;
            copy._isOID = _isOID;
            copy._weight = _weight;

            return copy;
        }
   }
}
