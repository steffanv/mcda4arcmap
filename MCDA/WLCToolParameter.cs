using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;

namespace MCDA.Model
{

   public class WLCToolParameter : INotifyPropertyChanged, IToolParameter, IDeepClonable<WLCToolParameter>
   {
        
        public event PropertyChangedEventHandler PropertyChanged;
        public event PropertyChangedEventHandler WeightPropertyChanged;
        public event PropertyChangedEventHandler BenefitPropertyChanged;
        
        private bool _isBenefitCriterion = false;
        private bool _isLocked = false;
        private double _weight = 0;
        private string _columnName;
        private bool _isOID = false;
       
        private static IToolParameter _lastWeightChangedToolParameter;

        private static bool _isPropertiesLocked;

         public WLCToolParameter(){}

         public void SetLockedWeight(double weight)
         {
             _weight = weight;
             PropertyChanged.Notify(() => Weight);
         }

       public bool IsPropertiesLocked{
       
           get {return _isPropertiesLocked;}
           set {_isPropertiesLocked = value;}
       }

       public double NonLockableWeight { 

           get{return _weight;} 
           set{PropertyChanged.ChangeAndNotify(ref _weight, value, () => Weight);}
       }

        public WLCToolParameter(string columnName )
        {
            _columnName = columnName;
        }
        public bool IsBenefitCriterion
        {
            get { return _isBenefitCriterion; }
            set { if(_isPropertiesLocked){ return;} BenefitPropertyChanged.ChangeAndNotify(ref _isBenefitCriterion, value, () => IsBenefitCriterion); }
        }

        public double Weight
        {
            get { return _weight; }
            set { if(_isPropertiesLocked){ return;} _lastWeightChangedToolParameter = this; WeightPropertyChanged.ChangeAndNotify(ref _weight, value, () => Weight); }
        }

        public bool IsLocked
        {
            get { return _isLocked; }
            set { if(_isPropertiesLocked){ return;} PropertyChanged.ChangeAndNotify(ref  _isLocked, value, () => IsLocked); }

        }
        public string ColumnName
        {
            get { return _columnName; }
            set { _columnName = value; }
        }

        public IToolParameter LastWeightChangedToolParameter
        {
            get
            {
                return _lastWeightChangedToolParameter;
            }
            set
            {
                _lastWeightChangedToolParameter = value;
            }
        }


        public bool IsOID
        {
            get
            {
                return _isOID;
            }
            set { PropertyChanged.ChangeAndNotify(ref  _isOID, value, () => IsOID); }
        }


        public double ScaledWeight
        {
            get
            {
                return _weight / 100;
            }
            set
            {
                _weight = value * 100;
            }
        }

        public WLCToolParameter DeepClone()
        {
            WLCToolParameter copy = new WLCToolParameter();

            copy._columnName = _columnName;
            copy._isBenefitCriterion = _isBenefitCriterion;
            copy._isLocked = _isLocked;
            copy._isOID = _isOID;
            copy._weight = _weight;

            return copy;
        }
   }
}
