using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using MCDA.Model;

namespace MCDA.Entity
{
   
    public class WLCParameter : INotifyPropertyChanged
    {

        public event PropertyChangedEventHandler PropertyChanged;

        private IList<WLCToolParameter> _listOfParameter;
        private IWeightDistributionStrategy _weightDistributionStrategy = WeightDistributionStrategyFactory.DefaultWeightDistributionStrategy();

        public WLCParameter(IList<WLCToolParameter> listOfParameter )
        {
            _listOfParameter = listOfParameter;

            _listOfParameter.ForEach(p => p.RegisterPropertyHandler(w => w.Weight, WeightPropertyChanged));

        }

        void WeightPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _weightDistributionStrategy.Distribute(_listOfParameter);
        }

        public IWeightDistributionStrategy WeightDistributionStrategy{

            get { return _weightDistributionStrategy;}
            set { _weightDistributionStrategy = value; }
            
        }

        public IList<WLCToolParameter> ToolParameter
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
