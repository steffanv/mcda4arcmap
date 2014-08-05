using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;
using MCDA.Misc;

namespace MCDA.Model
{

    internal sealed class ToolParameter : AbstractToolParameter
    {

        private static IToolParameter _lastWeightChangedToolParameter;
        private bool _isActive;

        public ToolParameter(string columnName)
        {
            _columnName = columnName;
        }

        public override IToolParameter LastWeightChangedToolParameter
        {
            get { return _lastWeightChangedToolParameter; }
            set { _lastWeightChangedToolParameter = value; }
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

        public override bool IsActive
        {
            get { return _isActive; }
            set
            {
                _isActive = value;

                OnPropertyChanged(new PropertyChangedEventArgs("IsActive"));
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

        public override Range<double> AcceptableWeightRange
        {
            get {

                double sum = ToolParameterContainer.ToolParameter.Where(p => !p.IsLocked && p != this).Sum(p => p.Weight);

                return new Range<double>() { Minimum = Math.Max(this.Weight - (100 - sum), 0), Maximum = this.Weight + sum };
            }
        }
    }
}
