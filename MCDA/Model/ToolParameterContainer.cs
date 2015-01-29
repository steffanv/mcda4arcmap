using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using MCDA.Extensions;

namespace MCDA.Model
{  
    internal sealed class ToolParameterContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private IList<IToolParameter> _listOfParameter;
        private IWeightDistributionStrategy _weightDistributionStrategy = WeightDistributionStrategyFactory.DefaultWeightDistributionStrategy();

        private readonly IList<PropertyChangedEventHandler> listOfpropertyChangedEventHandlersForToolParameterWeight = new List<PropertyChangedEventHandler>();

        private static bool _isLocked = false;

        public ToolParameterContainer(IList<IToolParameter> listOfParameter )
        {
            this._listOfParameter = listOfParameter;

            foreach (var currentParameter in this._listOfParameter)
            {
                currentParameter.RegisterPropertyHandler(x => x.Weight, WeightPropertyChanged);
                currentParameter.RegisterPropertyHandler(x => x.IsLocked, IsLockedPropertyChanged);

                currentParameter.IsActive = true;

                currentParameter.ToolParameterContainer = this;
            }

        }

        private void WeightPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_isLocked)
            {
                _isLocked = true;

                _weightDistributionStrategy.Distribute(_listOfParameter);

                _isLocked = false;

                PropertyChanged.Notify(() => ToolParameter);
            }

            if (_listOfParameter.Where(p => p.IsLocked).Sum(p => p.Weight) >= 100)
            {
                foreach (var item in _listOfParameter.Where(p => p.IsActive && !p.IsLocked))
                {
                    item.IsActive = false;
                }

                PropertyChanged.Notify(() => ToolParameter);                
            }

            if ((_listOfParameter.Where(p => p.IsLocked).Sum(p => p.Weight) < 100) && (_listOfParameter.Count(p => p.IsLocked) < _listOfParameter.Count - 1))
            {
                foreach (var item in _listOfParameter.Where(p => !p.IsActive && !p.IsLocked))
                {
                    item.IsActive = true;
                }

                PropertyChanged.Notify(() => ToolParameter);
            }
        }

        private void IsLockedPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((_listOfParameter.Count(p => p.IsLocked) >= _listOfParameter.Count - 1) || (_listOfParameter.Where(p => p.IsLocked).Sum(p => p.Weight) >= 100))
            {
                var parameter = _listOfParameter.FirstOrDefault(p => !p.IsLocked);

                if(parameter != null)
                {
                   parameter.IsActive = false;
                   PropertyChanged.Notify(() => ToolParameter);
                }                
            }

            if ((_listOfParameter.Any(p => p.IsActive == false) && _listOfParameter.Count(p => p.IsLocked) < _listOfParameter.Count - 1) && (_listOfParameter.Where(p => p.IsLocked).Sum(p => p.Weight) < 100))
            {
                foreach (var item in _listOfParameter.Where(p => !p.IsActive))
                {
                    item.IsActive = true;
                }

                PropertyChanged.Notify(() => ToolParameter);
            }
        }

        public IWeightDistributionStrategy WeightDistributionStrategy{

            get { return _weightDistributionStrategy;}
            set { _weightDistributionStrategy = value; }       
        }

        public void DistributeEquallyToolParameterWeights()
        {
            var weight = 100 / (double)_listOfParameter.Count;

            _isLocked = true;

            foreach (var currentParameter in _listOfParameter)
            {
                currentParameter.Weight = weight;
            }

            _isLocked = false;

            PropertyChanged.Notify(() => ToolParameter);
        }

        public IList<IToolParameter> ToolParameter
        {
            get { return _listOfParameter.Where(t => !t.IsOID).ToList(); }
            set {

                foreach (var currentParameter in _listOfParameter)
                {
                    currentParameter.UnRegisterPropertyHandler(listOfpropertyChangedEventHandlersForToolParameterWeight);
                }

                PropertyChanged.ChangeAndNotify(ref _listOfParameter, value, () => ToolParameter);

                foreach (var currentParameter in _listOfParameter)
                {
                    listOfpropertyChangedEventHandlersForToolParameterWeight.Add(
                        currentParameter.RegisterPropertyHandler(w => w.Weight, WeightPropertyChanged));
                }
            }
        }
    }
}
