using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;
using System.Threading;
using ESRI.ArcGIS.Carto;

namespace MCDA.Model
{
    /// <summary>
    /// Encapsulates a <see cref="ESRI.ArcGIS.Geodatabase.IFeatureClass"/> and if available the corresponding <see cref="ESRI.ArcGIS.Carto.IFeatureLayer2"/>
    /// </summary>
    internal sealed class Feature : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private string _featureName;
        private bool isFeatureLayer;
        private bool isSelected;
        //can be null
        private IFeatureLayer2 featureLayer;
        private IFeatureClass featureClass;
        private readonly ObservableCollection<Field> fields;
        private Field selectedFieldForRendering;
        private ESRI.ArcGIS.Carto.ILayer2 esriLayer;

        private static Feature lastSelectedLayer;


        public Feature(ESRI.ArcGIS.Carto.ILayer2 layer)
        {

            this.esriLayer = layer;
            _featureName = layer.Name;

            IFeatureLayer2 featureLayer = layer as ESRI.ArcGIS.Carto.IFeatureLayer2;

            if (featureLayer == null)
            {
                isFeatureLayer = false;
                UniqueLayerName = string.Empty;
                fields = new ObservableCollection<Field>();
            }
            else
            { 
                isFeatureLayer = true;
                this.featureLayer = featureLayer;
                UniqueLayerName = ToUniqueLayerName(featureLayer);
                fields = new ObservableCollection<Field>(GetFields());
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

            featureLayer.FeatureClass = featureClass;

            this.featureClass = featureClass;
            this.featureLayer = featureLayer;

            isFeatureLayer = true;
            UniqueLayerName = ToUniqueLayerName(this.featureLayer);

            this.esriLayer = featureLayer as ILayer2;
            _featureName = this.esriLayer.Name;

            fields = new ObservableCollection<Field>(GetFields());
            
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

        public ESRI.ArcGIS.Carto.ILayer2 ESRILayer
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

        public string FeatureName
        {
            get { return _featureName; }
            set { PropertyChanged.ChangeAndNotify(ref _featureName, value, () => FeatureName); }
        }

        public bool IsSelected { 

            get { return isSelected; /*lastSelectedLayer == this;*/ }
            set
            {
                //isSelected = value;

                //if (value)
                  //  lastSelectedLayer = this;

                PropertyChanged.ChangeAndNotify(ref isSelected, value, () => IsSelected);
            }
        }

        //public static Feature LastSelectedLayer{

        //    get { return lastSelectedLayer; }
        //}

        public ObservableCollection<Field> Fields
        {
            get { return fields; }  
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

        /// <summary>
        /// In case the underlying <see cref="IFeatureClass"/> is extended by a new field it is required to call this method.
        /// </summary>
        public void UpdateFieldsProperty()
        {
            Fields.Clear();

            foreach (var newField in GetFields())
                Fields.Add(newField);

            //PropertyChanged.Notify(() => Fields);
        }

        //TODO wozu ist das gut?
        private string ToUniqueLayerName(ESRI.ArcGIS.Carto.IFeatureLayer2 featureLayer)
        {
            return featureLayer.FeatureClass.Fields.ToString() + featureLayer.ToString();
        }

        //TODO think about it - is it necessary?
        //public void RegisterListenerForEveryMemberOfFields()
        //{
        //    foreach (var currentField in fields)
        //    {
        //        currentField.PropertyChanged -= new PropertyChangedEventHandler(FieldPropertyChanged);
        //    }

        //    foreach (var currentField in fields)
        //    {
        //        currentField.PropertyChanged +=new PropertyChangedEventHandler(FieldPropertyChanged);
        //    }
        //}

        //private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        //{
        //    PropertyChanged.Notify(() => Fields);
        //}

        private IList<Field> GetFields()
        {//TODO using?
            ESRI.ArcGIS.Geodatabase.IFields fields = featureLayer.FeatureClass.Fields;

            IList<Field> listOfFields = new List<Field>();

            for (int i = 0; i < fields.FieldCount; i++)
                listOfFields.Add(new Field(fields.get_Field(i),this));

            return listOfFields;
        }

    }
}
