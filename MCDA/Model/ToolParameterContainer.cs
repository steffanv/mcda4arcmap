using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using MCDA.Model;

namespace MCDA.Model
{  
    internal sealed class ToolParameterContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IList<IToolParameter> _listOfParameter;
        private IWeightDistributionStrategy _weightDistributionStrategy = WeightDistributionStrategyFactory.DefaultWeightDistributionStrategy();

        private IList<PropertyChangedEventHandler> listOfpropertyChangedEventHandlersForToolParameterWeight = new List<PropertyChangedEventHandler>();

        private static bool _isLocked = false;

        public ToolParameterContainer(IList<IToolParameter> listOfParameter )
        {
            this._listOfParameter = listOfParameter;

            foreach (var currentParameter in this._listOfParameter)
                currentParameter.RegisterPropertyHandler(x => x.Weight, WeightPropertyChanged);

        }

        void WeightPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_isLocked)
            {
                _isLocked = true;

                _weightDistributionStrategy.Distribute(_listOfParameter);

                _isLocked = false;

                PropertyChanged.Notify(() => ToolParameter);
            }    
        }

        public IWeightDistributionStrategy WeightDistributionStrategy{

            get { return _weightDistributionStrategy;}
            set { _weightDistributionStrategy = value; }       
        }

        public void DistributeEquallyToolParameterWeights()
        {
            double weight = 100 / (double)_listOfParameter.Count;

            _isLocked = true;

            foreach (var currentParameter in _listOfParameter)
                currentParameter.Weight = weight;

            _isLocked = false;

            PropertyChanged.Notify(() => ToolParameter);
        }

        public IList<IToolParameter> ToolParameter
        {
            get { return _listOfParameter.Where(t => !t.IsOID).ToList(); }
            set {

                foreach (var currentParameter in _listOfParameter)
                    currentParameter.UnRegisterPropertyHandler(listOfpropertyChangedEventHandlersForToolParameterWeight);

                PropertyChanged.ChangeAndNotify(ref _listOfParameter, value, () => ToolParameter);

                foreach (var currentParameter in _listOfParameter)
                    listOfpropertyChangedEventHandlersForToolParameterWeight.Add(currentParameter.RegisterPropertyHandler(w => w.Weight, WeightPropertyChanged));
            }
        }
    }
}
