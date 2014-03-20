using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;
using System.Threading;
using System.Diagnostics.Contracts;
using ESRI.ArcGIS.Carto;

namespace MCDA.Model
{
    /// <summary>
    /// Encapsulates a <see cref="ESRI.ArcGIS.Geodatabase.IFeatureClass"/> and if available the corresponding <see cref="ESRI.ArcGIS.Carto.IFeatureLayer2"/>
    /// </summary>
    internal sealed class Feature : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string layerName;
        private bool isFeatureLayer;
        private bool isSelected;
        //can be null
        private IFeatureLayer2 featureLayer;
        private IFeatureClass featureClass;
        private IList<Field> fields;
        private Field selectedFieldForRendering;
        private ESRI.ArcGIS.Carto.ILayer esriLayer;

        private static Feature lastSelectedLayer;


        public Feature(ESRI.ArcGIS.Carto.ILayer layer)
        {
            this.esriLayer = layer;
            layerName = layer.Name;

            IFeatureLayer2 featureLayer = layer as ESRI.ArcGIS.Carto.IFeatureLayer2;

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

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>
        /// The <see cref="IFeatureLayer.FeatureClass"/> attribute is set to the given <see cref="IFeatureLayer2"/>.
        /// </remarks>
        /// <param name="featureClass"></param>
        /// <param name="featureLayer"></param>
        /// <exception cref="ArgumentNullExeption"> </exception>
        public Feature(IFeatureClass featureClass, IFeatureLayer2 featureLayer)
        {
            Contract.Requires<ArgumentNullException>(featureClass != null);
            Contract.Requires<ArgumentNullException>(featureLayer != null);

            featureLayer.FeatureClass = featureClass;

            this.featureClass = featureClass;
            this.featureLayer = featureLayer;

            isFeatureLayer = true;
            UniqueLayerName = ToUniqueLayerName(this.featureLayer);

            this.esriLayer = featureLayer as ILayer;
            layerName = this.esriLayer.Name;

            fields = GetFields();
            
        }

        public string NotSuitableForMCDAReason
        {
            get
            {
                if (!IsFeatureLayer)
                    return "Feature is not a FeatureLayer.";
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
            get { return esriLayer; }
        }

        public ESRI.ArcGIS.Carto.IFeatureLayer2 FeatureLayer
        {
            get { return featureLayer; }
        }

        public string UniqueLayerName { get; set; }

        public Field SelectedFieldForRendering
        {
            get { return selectedFieldForRendering; }
            set { PropertyChanged.ChangeAndNotify(ref selectedFieldForRendering, value, () => SelectedFieldForRendering);}
        }

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

        public static Feature LastSelectedLayer{

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

        //TODO wozu ist das gut?
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
