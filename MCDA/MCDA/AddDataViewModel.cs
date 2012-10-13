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

       private BindingList<Layer> _layer;
       private BindingList<Field> _fields = new BindingList<Field>();

       private MCDAExtension mcdaExtension;

        public AddDataViewModel() 
        {
            mcdaExtension = MCDAExtension.GetExtension();

            _layer = new BindingList<Layer>(mcdaExtension.AvailableFeatureLayer);

            _layer.ListChanged += new ListChangedEventHandler(_layer_ListChanged);
            _fields.ListChanged += new ListChangedEventHandler(_fields_ListChanged);
           
            mcdaExtension.PropertyChanged += new PropertyChangedEventHandler(mcdaExtension_PropertyChanged);

            
        }

        public BindingList<Layer> Layer
        {
            get { return _layer; }
            set { _layer = value; }
        }

        public BindingList<Field> Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }

        void _fields_ListChanged(object sender, ListChangedEventArgs e)
        {
            Update();
        }

        void _layer_ListChanged(object sender, ListChangedEventArgs e)
        {
            Update();
        }

       

       /// <summary>
       /// Updates and binds the new list of layer from the model to the view.
       /// </summary>
       /// <param name="sender"></param>
       /// <param name="e"></param>
        void mcdaExtension_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
           
            Update();

          
        }
      
        private void Update()
        {

            _layer.ListChanged -= new ListChangedEventHandler(_layer_ListChanged);

            _layer = new BindingList<Layer>(mcdaExtension.AvailableFeatureLayer);

            _layer.ListChanged += new ListChangedEventHandler(_layer_ListChanged);

            _fields.ListChanged -= new ListChangedEventHandler(_fields_ListChanged);
            _fields = new BindingList<Field>();
           
            _layer.Where(l => l.IsSelected).ForEach(l => l.Fields.Where(f => f.IsNumber).ForEach(f => _fields.Add(f)));

            _fields.ListChanged += new ListChangedEventHandler(_fields_ListChanged);

            PropertyChanged.Notify(() => Layer);
            PropertyChanged.Notify(() => Fields);
        }
    }
}
