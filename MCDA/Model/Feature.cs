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
        private bool _isFeatureLayer;
        private bool _isSelected;
        private readonly IFeatureLayer2 _featureLayer;
        private IFeatureClass _featureClass;
        private readonly ObservableCollection<Field> _fields;
        private Field _selectedFieldForRendering;
        private readonly ESRI.ArcGIS.Carto.ILayer2 _esriLayer;

        public Feature(ESRI.ArcGIS.Carto.ILayer2 layer)
        {
            this._esriLayer = layer;
            _featureName = layer.Name;

            var featureLayer = layer as ESRI.ArcGIS.Carto.IFeatureLayer2;

            if (featureLayer == null)
            {
                _isFeatureLayer = false;
                _fields = new ObservableCollection<Field>();
            }
            else
            { 
                _isFeatureLayer = true;
                this._featureLayer = featureLayer;
                _fields = new ObservableCollection<Field>(GetFields().OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(l => l.FieldName));
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

            this._featureClass = featureClass;
            this._featureLayer = featureLayer;

            _isFeatureLayer = true;

            this._esriLayer = featureLayer as ILayer2;
            _featureName = this._esriLayer.Name;

            _fields = new ObservableCollection<Field>(GetFields().OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(l => l.FieldName));
            
        }

        public string NotSuitableForMCDAReason
        {
            get
            {
                if (!IsFeatureLayer)
                    return "Feature has not all FeatureLayer properties.";
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
            get { return _featureLayer.FeatureClass; }
        }

        public ESRI.ArcGIS.Carto.ILayer2 ESRILayer
        {
            get { return _esriLayer; }
        }

        public ESRI.ArcGIS.Carto.IFeatureLayer2 FeatureLayer
        {
            get { return _featureLayer; }
        }

        public Field SelectedFieldForRendering
        {
            get { return _selectedFieldForRendering; }
            set { PropertyChanged.ChangeAndNotify(ref _selectedFieldForRendering, value, () => SelectedFieldForRendering);}
        }

        public bool IsFeatureLayer
        {
            get { return _isFeatureLayer; }
            set { PropertyChanged.ChangeAndNotify(ref _isFeatureLayer, value, () => IsFeatureLayer);}
        }

        public string FeatureName
        {
            get { return _featureName; }
            set { PropertyChanged.ChangeAndNotify(ref _featureName, value, () => FeatureName); }
        }

        public bool IsSelected { 

            get { return _isSelected; }
            set { PropertyChanged.ChangeAndNotify(ref _isSelected, value, () => IsSelected); }
        }

        public ObservableCollection<Field> Fields
        {
            get { return _fields; }  
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

            foreach (var newField in GetFields().OrderByDescending(f => f.IsSuitableForMCDA).ThenBy(l => l.FieldName))
                Fields.Add(newField);
        }

        public void SetToolField(IField field)
        {
            var foundField = Fields.FirstOrDefault(f => f.ESRIField == field);

            if (foundField != null)
                foundField.IsToolField = true;
        }

        private IEnumerable<Field> GetFields()
        {
            var fields = _featureLayer.FeatureClass.Fields;

            IList<Field> listOfFields = new List<Field>();

            for (int i = 0; i < fields.FieldCount; i++)
                listOfFields.Add(new Field(fields.Field[i],this));

            return listOfFields;
        }
    }
}
