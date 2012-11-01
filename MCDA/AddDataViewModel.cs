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
    /// <summary>
    /// 
    /// </summary>
   public class AddDataViewModel : INotifyPropertyChanged
    {
       public event PropertyChangedEventHandler PropertyChanged;

       private BindingList<Feature> _features;
       private BindingList<Field> _fields = new BindingList<Field>();

       private MCDAExtension mcdaExtension;

        public AddDataViewModel() 
        {
            mcdaExtension = MCDAExtension.GetExtension();

            _features = new BindingList<Feature>(mcdaExtension.AvailableFeatureLayer);

            mcdaExtension.PropertyChanged += new PropertyChangedEventHandler(mcdaExtensionPropertyChanged);

            //call because the extension could already have a selected feature and thus fiels
            mcdaExtensionPropertyChanged(this,null);
  
        }

        void mcdaExtensionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {         
            _features = new BindingList<Feature>(mcdaExtension.AvailableFeatureLayer);

            Feature feature = _features.Where(l => l.IsSelected).FirstOrDefault();

            if (feature != null)
            {
                List<Field> field = feature.Fields.Where(f => f.IsNumber).ToList();

                _fields = new BindingList<Field>(field);
            }

            PropertyChanged.Notify(() => Layer);
            PropertyChanged.Notify(() => Fields);
        }

        public BindingList<Feature> Layer
        {
            get { return _features; }
            set { _features = value; }
        }

        public BindingList<Field> Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }
    }
}
