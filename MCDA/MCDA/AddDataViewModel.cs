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

        private MCDAExtension mcdaExtension;

        public AddDataViewModel()
        {
            mcdaExtension = MCDAExtension.GetExtension();

            mcdaExtension.RegisterPropertyHandler(x => x.AvailableFeatures, AvailableFeaturesListChanged);

            //call because the extension could already have a selected feature and thus fields
            AvailableFeaturesListChanged(this, null);
        }

        public BindingList<Feature> Layer { get; set; }

        public BindingList<Field> Fields { get; set; }

        private void AvailableFeaturesListChanged(object sender, PropertyChangedEventArgs e)
        {
            Layer = new BindingList<Feature>(mcdaExtension.AvailableFeatures.OrderByDescending(l => l.IsSuitableForMCDA).ThenBy(l => l.LayerName).ToList());

            Feature feature = Layer.Where(l => l.IsSelected).FirstOrDefault();

            if (feature != null)
            {
                feature.RegisterPropertyHandler(x => x.Fields, FieldsListChanged);
                Fields = new BindingList<Field>(feature.Fields.OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(f => f.FieldName).ToList());
            }
            else
                Fields = new BindingList<Field>();

            PropertyChanged.Notify(() => Layer);
            PropertyChanged.Notify(() => Fields);
        }

        private void FieldsListChanged(object sender, PropertyChangedEventArgs e)
        {
            Feature feature = Layer.Where(l => l.IsSelected).FirstOrDefault();

            if (feature != null)
            {
                feature.RegisterPropertyHandler(x => x.Fields, FieldsListChanged);
                Fields = new BindingList<Field>(feature.Fields.OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(f => f.FieldName).ToList());
            }
            else
                Fields = new BindingList<Field>();

            PropertyChanged.Notify(() => Fields);
        }

    }
}
