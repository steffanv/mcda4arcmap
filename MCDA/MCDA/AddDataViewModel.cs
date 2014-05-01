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

        private IList<PropertyChangedEventHandler> listOfpropertyChangedEventHandlersForFeatureIsSelected = new List<PropertyChangedEventHandler>();

        public AddDataViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _mcdaExtension.AvailableFeatures.CollectionChanged += AvailableFeaturesCollectionChanged;

            Features = new BindingList<Feature>();
            Fields = new BindingList<Field>();

            //call because the extension could already have a selected feature and thus fields
            AvailableFeaturesCollectionChanged(this, null);
        }

        public BindingList<Feature> Features { get; set; }

        public BindingList<Field> Fields { get; set; }

        private void AvailableFeaturesCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            foreach (var feature in Features)
                feature.UnRegisterPropertyHandler(listOfpropertyChangedEventHandlersForFeatureIsSelected);

            Features = new BindingList<Feature>(_mcdaExtension.AvailableFeatures.OrderByDescending(l => l.IsSuitableForMCDA).ThenBy(l => l.FeatureName).ToList());

            foreach (var feature in Features)
                listOfpropertyChangedEventHandlersForFeatureIsSelected.Add(feature.RegisterPropertyHandler(f => f.IsSelected, FeatureSelectionChanged));
              
            Feature selectedFeature = Features.FirstOrDefault(l => l.IsSelected);

            if (selectedFeature != null)
                Fields = new BindingList<Field>(selectedFeature.Fields.OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(f => f.FieldName).ToList());

            else
                Fields = new BindingList<Field>();

            PropertyChanged.Notify(() => Features);
            PropertyChanged.Notify(() => Fields);
        }

        public void FeatureSelectionChanged(object sender, PropertyChangedEventArgs e)
        {
            Feature selectedFeature = Features.FirstOrDefault(l => l.IsSelected);

            if (selectedFeature != null)
                Fields = new BindingList<Field>(selectedFeature.Fields.OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(f => f.FieldName).ToList());

            else
                Fields = new BindingList<Field>();

            PropertyChanged.Notify(() => Fields);
        }
    }
}
