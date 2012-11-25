using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using MCDA.Model;

namespace MCDA.Entity
{  
    public class ToolParameterContainer : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private IList<IToolParameter> _listOfParameter;
        private IWeightDistributionStrategy _weightDistributionStrategy = WeightDistributionStrategyFactory.DefaultWeightDistributionStrategy();

        private static bool _isLocked = false;

        public ToolParameterContainer(IList<IToolParameter> listOfParameter )
        {
            _listOfParameter = listOfParameter;

            _listOfParameter.ForEach(p => p.RegisterPropertyHandler(x => x.Weight, WeightPropertyChanged));

        }

        void WeightPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_isLocked)
            {
                _isLocked = true;
                _weightDistributionStrategy.Distribute(_listOfParameter);
            }

            _isLocked = false;
        }

        public IWeightDistributionStrategy WeightDistributionStrategy{

            get { return _weightDistributionStrategy;}
            set { _weightDistributionStrategy = value; }
            
        }

        public IList<IToolParameter> ToolParameter
        {
            get { return _listOfParameter.Where(t => !t.IsOID).ToList(); }
            set {

                _listOfParameter.ForEach(p => p.UnRegisterPropertyHandler(w => w.Weight, WeightPropertyChanged));
                PropertyChanged.ChangeAndNotify(ref _listOfParameter, value, () => ToolParameter);
                _listOfParameter.ForEach(p => p.RegisterPropertyHandler(w => w.Weight, WeightPropertyChanged));
            }
        }

    }

}
