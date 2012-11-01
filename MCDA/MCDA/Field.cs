using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Extensions;
using System.ComponentModel;

namespace MCDA.Model
{
    public class Field : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool _isSelected;
        private bool _isOID = false;
        private ESRI.ArcGIS.Geodatabase.IField _field;
        private bool _isNumber = false;
        private Feature _layer;

        public Field(ESRI.ArcGIS.Geodatabase.IField field, Feature layer)
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

        public string FieldName
        {
            get { return _field.Name; }
        }

        public bool IsNumber
        {
            get { return _isNumber; }
        }

        public Feature Layer{

            get { return _layer; }
        }
        public bool IsOID
        {
            get { return _isOID; }
        }
    }
}
