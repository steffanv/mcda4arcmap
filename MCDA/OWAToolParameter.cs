using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MCDA.Model
{
   public class OWAToolParameter : AbstractToolParameter
    {

       private static IToolParameter _lastWeightChangedToolParameter;

       public OWAToolParameter(string columnName)
       {
           _columnName = columnName;
       }

       public override double Weight
       {
           get { return _weight; }
           set
           {
               _lastWeightChangedToolParameter = this;
               _weight = value;
               OnPropertyChanged(new PropertyChangedEventArgs("Weight"));
           }
       }

       public override IToolParameter LastWeightChangedToolParameter
       {
           get { return _lastWeightChangedToolParameter; }
           set { _lastWeightChangedToolParameter = value; }
       }

        public override IToolParameter DeepClone()
        {
            OWAToolParameter copy = new OWAToolParameter(_columnName);

            copy._isBenefitCriterion = _isBenefitCriterion;
            copy._isLocked = _isLocked;
            copy._isOID = _isOID;
            copy._weight = _weight;

            return copy;
        }
    }
}
