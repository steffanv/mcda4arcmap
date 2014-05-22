using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using MCDA.Extensions;
using MCDA.Model;

namespace MCDA.ViewModel
{
    internal sealed class AddDataViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MCDAExtension _mcdaExtension;

        private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForFeatureIsSelected = new List<PropertyChangedEventHandler>();

        public AddDataViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _mcdaExtension.AvailableFeatures.CollectionChanged += AvailableFeaturesCollectionChanged;

            Features = new ObservableCollection<Feature>();

            //call because the extension could already have a selected feature and thus fields
            AvailableFeaturesCollectionChanged(this, null);
        }

        public ObservableCollection<Feature> Features { get; set; }

        private void AvailableFeaturesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var feature in Features)
                feature.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFeatureIsSelected);

            Features = new ObservableCollection<Feature>(_mcdaExtension.AvailableFeatures.OrderByDescending(l => l.IsSuitableForMCDA).ThenBy(l => l.FeatureName).ToList());

            PropertyChanged.Notify(() => Features);
        }     
    }
}
