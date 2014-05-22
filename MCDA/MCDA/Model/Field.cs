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
        private readonly bool _isOid = false;
        private readonly bool _isNumber = false;
        private readonly ESRI.ArcGIS.Geodatabase.IField _field;
        private RendererContainer _renderContainer;

        public Field(ESRI.ArcGIS.Geodatabase.IField field, Feature feature)
        {

            Feature = feature;
            this._field = field;

            _renderContainer = new RendererContainer(this);
   
            if (field.Type <= ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeDouble)
                _isNumber = true;
            if (field.Type == ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeOID)
                _isOid = true;
        }

        public RendererContainer RenderContainer {

            get { return _renderContainer; }
            set { PropertyChanged.ChangeAndNotify(ref _renderContainer, value, () => RenderContainer); }     
        }

        /// <summary>
        /// Returns a string representation why this Field is not suitable. Returns an empty string if the Field is suitable.
        /// </summary>
        public string NotSuitableForMCDAReason
        {
            get
            {
                if (!IsNumeric) 
                    return "Field is not numeric.";
                if (ContainsNullValue())
                    return "Field contains NULL values.";
                if (!HasDifferentNumericValues())
                    return "Field has no distinct values.";

                return string.Empty;
            }
        }
        /// <summary>
        /// Returns true if the Field is numeric, does not have NULL values and has different numeric values.
        /// </summary>
        public bool IsSuitableForMCDA
        {
            get { return IsNumeric && !ContainsNullValue() && HasDifferentNumericValues(); }
        }
 
        public bool IsSelected
        {
            get { return _isSelected; }
            set { PropertyChanged.ChangeAndNotify(ref _isSelected, value, () => IsSelected); }
        }

        public ESRI.ArcGIS.Geodatabase.IField ESRIField 
        {
            get { return _field; }       
        }

        public string FieldName
        {
            get { return _field.Name; }
        }

        public bool IsNumeric
        {
            get { return _isNumber; }
        }

        public Feature Feature { get; private set; }

        public bool IsOID
        {
            get { return _isOid; }
        }

        private bool ContainsNullValue()
        {
            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = Feature.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                var fieldIndex = Feature.FeatureClass.FindField(FieldName);

                IFeature currentFeature; 

                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    if(currentFeature.Value[fieldIndex] is DBNull)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Determines if the values of the Field have at least one value that is distinct from all other values. A non numeric field returns always false.
        /// </summary>
        /// <returns></returns>
        private bool HasDifferentNumericValues()
        {
            if(!IsNumeric)
                return false;

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = Feature.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                var fieldIndex = Feature.FeatureClass.FindField(FieldName);

                var currentFeature = featureCursor.NextFeature();

                double value = 0;

                if (currentFeature != null)
                    value = (Convert.ToDouble(currentFeature.Value[fieldIndex]));
                else
                    return false;

                while ((currentFeature = featureCursor.NextFeature()) != null)
                {                   
                    double t = (Convert.ToDouble(currentFeature.Value[fieldIndex]));

                    if (t != value)
                        return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Returns the values in case the field is numeric, otherwise the IEnumerable is empty.
        /// DBNull values are skipped.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<double> GetFieldData()
        {
            if (!IsNumeric)
                return Enumerable.Empty<double>();

            IList<double> data = new List<double>();

            using (var comReleaser = new ComReleaser())
            {
                var featureCursor = Feature.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                var fieldIndex = Feature.FeatureClass.FindField(FieldName);

                var currentFeature = featureCursor.NextFeature();

                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    if(currentFeature.Value[fieldIndex] is DBNull)
                        continue;

                    double t = (Convert.ToDouble(currentFeature.Value[fieldIndex]));

                    data.Add(t);
                   
                }
            }

            return data.AsEnumerable();
        }

        public void PromoteToSelectedFieldForRendering()
        {
            Feature.SelectedFieldForRendering = this;
        }

        /// <summary>
        /// Demotes the <see cref="Field"/> as selected Field for the parent <see cref="Feature"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="Feature.SelectedFieldForRendering"/> is set to <code>null</code> only if the the <see cref="Field"/> is the actual selected field for rendering.
        /// </remarks>
        public void DemoteAsSelectedFieldForRendering()
        {
            if (Feature.SelectedFieldForRendering == this)
                Feature.SelectedFieldForRendering = null;
        }

        public bool IsSelectedFieldForRendering
        {
            get { return Feature.SelectedFieldForRendering == this; }
        } 
    }
}
