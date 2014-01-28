using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.ADF;

namespace MCDA.Model
{
    internal sealed class Field : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        private bool _isOID = false;
        private bool _isNumber = false;
        private ESRI.ArcGIS.Geodatabase.IField _field;
        private Layer _layer;

        public Field(ESRI.ArcGIS.Geodatabase.IField field, Layer layer)
        {
            _layer = layer;
            _field = field;
   
            if (_field.Type <= ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeDouble)
                _isNumber = true;
            if (_field.Type == ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeOID)
                _isOID = true;
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set { PropertyChanged.ChangeAndNotify(ref _isSelected, value, () => IsSelected); }
        }

        public ESRI.ArcGIS.Geodatabase.IField ESRIField { get { return _field; } }

        public string FieldName
        {
            get { return _field.Name; }
        }

        public bool IsNumber
        {
            get { return _isNumber; }
        }

        public Layer Layer{

            get { return _layer; }
        }

        public bool IsOID
        {
            get { return _isOID; }
        }

        public bool HasDifferentNumericValues()
        {
            if(!IsNumber)
                return false;

            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureCursor featureCursor = _layer.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                int fieldIndex = _layer.FeatureClass.FindField(FieldName);

                IFeature currentFeature = featureCursor.NextFeature();

                double value = 0;
                if (currentFeature != null)
                {
                    value = (Convert.ToDouble(currentFeature.get_Value(fieldIndex)));
                }
                else
                    return false;

                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    double t = (Convert.ToDouble(currentFeature.get_Value(fieldIndex)));

                    if (t != value)
                        return true;
                }

                return false;
            }
        }
    }
}
