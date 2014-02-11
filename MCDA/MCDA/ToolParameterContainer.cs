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

        private IList<IToolParameter> listOfParameter;
        private IWeightDistributionStrategy _weightDistributionStrategy = WeightDistributionStrategyFactory.DefaultWeightDistributionStrategy();

        private static bool _isLocked = false;

        public ToolParameterContainer(IList<IToolParameter> listOfParameter )
        {
            this.listOfParameter = listOfParameter;

            foreach (var currentParameter in this.listOfParameter)
            {
                currentParameter.RegisterPropertyHandler(x => x.Weight, WeightPropertyChanged);
            }

        }

        void WeightPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!_isLocked)
            {
                _isLocked = true;

                _weightDistributionStrategy.Distribute(listOfParameter);

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
            double weight = 100 / (double)listOfParameter.Count;

            _isLocked = true;

            foreach (var currentParameter in listOfParameter)
            {
                currentParameter.Weight = weight;
            }

            _isLocked = false;

            PropertyChanged.Notify(() => ToolParameter);
        }

        public IList<IToolParameter> ToolParameter
        {
            get { return listOfParameter.Where(t => !t.IsOID).ToList(); }
            set {

                foreach (var currentParameter in listOfParameter)
                {
                    currentParameter.UnRegisterPropertyHandler(w => w.Weight, WeightPropertyChanged);
                }
                PropertyChanged.ChangeAndNotify(ref listOfParameter, value, () => ToolParameter);

                foreach (var currentParameter in listOfParameter)
                {
                    currentParameter.RegisterPropertyHandler(w => w.Weight, WeightPropertyChanged);
                }
            }
        }
    }
}
