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
    internal sealed class Layer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string layerName;
        private bool isFeatureLayer;
        private bool isSelected;
        //can be null
        private ESRI.ArcGIS.Carto.IFeatureLayer2 featureLayer;
        private IList<Field> fields;
        private ESRI.ArcGIS.Carto.ILayer layer;

        private static Layer lastSelectedLayer;


        public Layer(ESRI.ArcGIS.Carto.ILayer layer)
        {
            this.layer = layer;
            layerName = layer.Name;

            ESRI.ArcGIS.Carto.IFeatureLayer2 featureLayer = layer as ESRI.ArcGIS.Carto.IFeatureLayer2;

            if (featureLayer == null)
            {
                isFeatureLayer = false;
                UniqueLayerName = string.Empty;
                fields = new List<Field>();
            }
            else
            { 
                isFeatureLayer = true;
                this.featureLayer = featureLayer;
                UniqueLayerName = ToUniqueLayerName(featureLayer);
                fields = GetFields();
            }
        }

        public string NotSuitableForMCDAReason
        {
            get
            {
                if (!IsFeatureLayer)
                    return "Layer is not a FeatureLayer.";
                if (!HasAreaAndTopologicalOperator())
                    return "Geometry must be Polygon.";
                else return string.Empty;

            }
        }

        public bool IsSuitableForMCDA
        {
            get { return IsFeatureLayer && HasAreaAndTopologicalOperator() ; }
        }

        public IFeatureClass FeatureClass
        {
            get { return featureLayer.FeatureClass; }
        }

        public ESRI.ArcGIS.Carto.ILayer ESRILayer
        {
            get { return layer; }
        }

        public ESRI.ArcGIS.Carto.IFeatureLayer2 FeatureLayer
        {
            get { return featureLayer; }
        }

        public string UniqueLayerName { get; set; }

        public bool IsFeatureLayer
        {
            get { return isFeatureLayer; }
            set { PropertyChanged.ChangeAndNotify(ref isFeatureLayer, value, () => IsFeatureLayer);}
        }

        public string LayerName
        {
            get { return layerName; }
            set { PropertyChanged.ChangeAndNotify(ref layerName, value, () => LayerName); }
        }

        public bool IsSelected { 

            get { return isSelected; }
            set {

                if (value)
                    lastSelectedLayer = this;

                PropertyChanged.ChangeAndNotify(ref isSelected, value, () => IsSelected);
            }
        }

        public static Layer LastSelectedLayer{

            get { return lastSelectedLayer; }
        }

        public IList<Field> Fields
        {
            get{ return fields.OrderBy(f => f.FieldName).ToList(); }
         
        }

        public bool HasAreaAndTopologicalOperator()
        {
            switch (FeatureLayer.ShapeType)
            {
                case ESRI.ArcGIS.Geometry.esriGeometryType.esriGeometryPolygon:
                    return true;
            }

            return false;
        }

        private string ToUniqueLayerName(ESRI.ArcGIS.Carto.IFeatureLayer2 featureLayer)
        {
            return featureLayer.FeatureClass.Fields.ToString() + featureLayer.ToString();
        }

        public void RegisterListenerForEveryMemberOfFields()
        {
            foreach (var currentField in fields)
            {
                currentField.PropertyChanged -= new PropertyChangedEventHandler(FieldPropertyChanged);
            }

            foreach (var currentField in fields)
            {
                currentField.PropertyChanged +=new PropertyChangedEventHandler(FieldPropertyChanged);
            }
        }

        private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            PropertyChanged.Notify(() => Fields);
        }

        private IList<Field> GetFields()
        {
            ESRI.ArcGIS.Geodatabase.IFields fields = featureLayer.FeatureClass.Fields;

            IList<Field> listOfFields = new List<Field>();

            for (int i = 0; i < fields.FieldCount; i++)
            {
                listOfFields.Add(new Field(fields.get_Field(i),this));
            }

            return listOfFields;
        }

    }
}
