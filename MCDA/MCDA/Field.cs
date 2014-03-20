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

        private bool isSelected;
        private bool isOID = false;
        private bool isNumber = false;
        private ESRI.ArcGIS.Geodatabase.IField field;
        private RendererContainer renderContainer;

        public Field(ESRI.ArcGIS.Geodatabase.IField field, Feature feature)
        {
            Feature = feature;
            this.field = field;

            renderContainer = new RendererContainer(this);
   
            if (field.Type <= ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeDouble)
                isNumber = true;
            if (field.Type == ESRI.ArcGIS.Geodatabase.esriFieldType.esriFieldTypeOID)
                isOID = true;
        }

        public RendererContainer RenderContainer {

            get { return renderContainer; }
            set { PropertyChanged.ChangeAndNotify(ref renderContainer, value, () => RenderContainer); }     
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
                if (!HasDifferentNumericValues())
                    return "Field has no distinct values.";
                else return string.Empty; 
            
            }
        }
        /// <summary>
        /// Returns true if the Field is numeric and has different numeric values.
        /// </summary>
        public bool IsSuitableForMCDA
        {
            get { return IsNumeric && HasDifferentNumericValues(); }
        }

        public bool IsSelected
        {
            get { return isSelected; }
            set { PropertyChanged.ChangeAndNotify(ref isSelected, value, () => IsSelected); }
        }

        public ESRI.ArcGIS.Geodatabase.IField ESRIField 
        {
            get { return field; }       
        }

        public string FieldName
        {
            get { return field.Name; }
        }

        public bool IsNumeric
        {
            get { return isNumber; }
        }

        public Feature Feature { get; private set; }

        public bool IsOID
        {
            get { return isOID; }
        }

        /// <summary>
        /// Determines if the values of the Field have at least one value that is distinct from all other values. A non numeric field returns always false.
        /// </summary>
        /// <returns></returns>
        public bool HasDifferentNumericValues()
        {
            if(!IsNumeric)
                return false;

            using (ComReleaser comReleaser = new ComReleaser())
            {
                IFeatureCursor featureCursor = Feature.FeatureClass.Search(null, true);

                comReleaser.ManageLifetime(featureCursor);

                int fieldIndex = Feature.FeatureClass.FindField(FieldName);

                IFeature currentFeature = featureCursor.NextFeature();

                double value = 0;

                if (currentFeature != null)
                    value = (Convert.ToDouble(currentFeature.get_Value(fieldIndex)));
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
