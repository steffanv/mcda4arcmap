using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;
using System.Threading;

namespace MCDA.Model
{
    public class Layer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _layerName;
        private string _uniqueLayerName;
        private bool _isFeatureLayer;
        private bool _isSelected;
        //can be null
        private ESRI.ArcGIS.Carto.IFeatureLayer2 _featureLayer;
        private IList<Field> _fields;
        private ESRI.ArcGIS.Carto.ILayer _layer;

        private static Layer _lastSelectedLayer;


        public Layer(ESRI.ArcGIS.Carto.ILayer layer)
        {
            _layer = layer;
            _layerName = layer.Name;

            ESRI.ArcGIS.Carto.IFeatureLayer2 featureLayer = layer as ESRI.ArcGIS.Carto.IFeatureLayer2;

            if (featureLayer == null)
            {
                _isFeatureLayer = false;
                _uniqueLayerName = string.Empty;
                _fields = new List<Field>();
            }
            else
            { 
                _isFeatureLayer = true;
                _featureLayer = featureLayer;
                _uniqueLayerName = toUniqueLayerName(featureLayer);
                _fields = GetFields();
            }

            //RegisterListenerForEveryMemberOfFields();
        }

        public ESRI.ArcGIS.Carto.ILayer ESRILayer
        {
            get { return _layer; }
        }

        public static string toUniqueLayerName(ESRI.ArcGIS.Carto.IFeatureLayer2 featureLayer)
        {

            return featureLayer.FeatureClass.Fields.ToString() + featureLayer.ToString();
        }

        public ESRI.ArcGIS.Carto.IFeatureLayer2 FeatureLayer
        {

            get { return _featureLayer; }
            private set { ;}
        }

        public string UniqueLayerName
        {

            get { return _uniqueLayerName; }
            set { _uniqueLayerName = value; }
        }

        public bool IsFeatureLayer
        {
            get { return _isFeatureLayer; }
            set { PropertyChanged.ChangeAndNotify(ref _isFeatureLayer, value, () => IsFeatureLayer);}
        }

        public string LayerName
        {

            get { return _layerName; }
            set { PropertyChanged.ChangeAndNotify(ref _layerName, value, () => LayerName); }
        }

        public bool IsSelected { 

            get{ return _isSelected; }
            set {

                if (value)
                    _lastSelectedLayer = this;

                PropertyChanged.ChangeAndNotify(ref _isSelected, value, () => IsSelected);
            }
        }

        public static Layer LastSelectedLayer{

            get { return _lastSelectedLayer; }
        }

        public IList<Field> Fields
        {
            get{ return _fields.OrderBy(f => f.FieldName).ToList(); }
         
        }

        public void RegisterListenerForEveryMemberOfFields()
        {
            _fields.ForEach(f => f.PropertyChanged -= new PropertyChangedEventHandler(f_PropertyChanged));
            _fields.ForEach(f => f.PropertyChanged +=new PropertyChangedEventHandler(f_PropertyChanged));
        }

        void f_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged.Notify(() => Fields);
        }

        private IList<Field> GetFields()
        {
            ESRI.ArcGIS.Geodatabase.IFields fields = _featureLayer.FeatureClass.Fields;

            IList<Field> listOfFields = new List<Field>();

            for (int i = 0; i < fields.FieldCount; i++)
            {
                listOfFields.Add(new Field(fields.get_Field(i),this));
            }

            return listOfFields;
        }

    }
}
