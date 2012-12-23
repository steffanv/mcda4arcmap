using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;

namespace MCDA.Model
{
   public abstract class AbstractToolParameter :  IToolParameter
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool _isBenefitCriterion = false;
        protected bool _isLocked = false;
        protected double _weight = 0;
        protected string _columnName;
        protected bool _isOID = false;

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
                handler(this, e);
        }

        public abstract double Weight { get; set; }

        public abstract IToolParameter LastWeightChangedToolParameter { get; set; }

        public bool IsOID
        {
            get { return _isOID; }
            set { PropertyChanged.ChangeAndNotify(ref  _isOID, value, () => IsOID); }
        }

        public double ScaledWeight
        {
            get { return _weight / 100; }
            set { _weight = value * 100; }
        }

        public bool IsBenefitCriterion
        {
            get { return _isBenefitCriterion; }
            set { PropertyChanged.ChangeAndNotify(ref _isBenefitCriterion, value, () => IsBenefitCriterion); }
        }

        public bool IsLocked
        {
            get { return _isLocked; }
            set {  PropertyChanged.ChangeAndNotify(ref  _isLocked, value, () => IsLocked); }

        }

        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public abstract IToolParameter DeepClone();
    }
}
