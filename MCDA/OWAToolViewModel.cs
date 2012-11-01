using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Model;
using MCDA.Extensions;

namespace MCDA.ViewModel
{
    public class OWAToolViewModel  : INotifyPropertyChanged
    {

       public event PropertyChangedEventHandler PropertyChanged;

       private MCDAExtension mcdaExtension;
       private BindingList<Field> _fields = new BindingList<Field>();
       private BindingList<Feature> _layer = new BindingList<Feature>();

       public OWAToolViewModel()
       {
           mcdaExtension = MCDAExtension.GetExtension();

           _layer = new BindingList<Feature>(mcdaExtension.AvailableLayer.Where(l => l.IsFeatureLayer).ToList());

           //register for events from the model to view always the latest list
           mcdaExtension.PropertyChanged += new PropertyChangedEventHandler(mcdaExtension_PropertyChanged);

           FillFieldList();
       }

       private void FillFieldList()
       {
           _fields = new BindingList<Field>();
           //get all fields from selected layer and add the number fields to the _field list
           _layer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.IsNumber).ForEach(f => _fields.Add(f)));
       }

       public BindingList<Field> Fields
       {
           get { return _fields; }
           set { _fields = value; }
       }

      
       void mcdaExtension_PropertyChanged(object sender, PropertyChangedEventArgs e)
       {
           _layer = new BindingList<Feature>(mcdaExtension.AvailableLayer.Where(l => l.IsFeatureLayer).ToList());

           FillFieldList();
           PropertyChanged.Notify(() => Fields);
       }
    }
}
