using System;
using System.Linq;
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
            var copy = new ToolParameter(_columnName)
            {
                _isBenefitCriterion = _isBenefitCriterion,
                _isLocked = _isLocked,
                _isOID = _isOID,
                _weight = _weight
            };

            return copy;
        }

        public override Range<double> AcceptableWeightRange
        {
            get {

                var sum = ToolParameterContainer.ToolParameter.Where(p => !p.IsLocked && p != this).Sum(p => p.Weight);

                return new Range<double>() { Minimum = Math.Max(this.Weight - (100 - sum), 0), Maximum = this.Weight + sum };
            }
        }
    }
}
