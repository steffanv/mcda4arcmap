using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Media;
using ESRI.ArcGIS.esriSystem;
using MCDA.Extensions;
using ESRI.ArcGIS.Geodatabase;

namespace MCDA.Model
{
    internal sealed class ClassBreaksRendererContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _startColor;
        private Color _endColor;

        private IClassify _classificationMethod;
        private int _numberOfClasses;

        private IField _field;

        private int _bins;

        public Color StartColor
        {
            get { return _startColor; }
            set { PropertyChanged.ChangeAndNotify(ref _startColor, value, () => StartColor); }
        }

        public Color EndColor
        {
            get { return _endColor; }
            set { PropertyChanged.ChangeAndNotify(ref _endColor, value, () => EndColor); }
        }

        public IClassify ClassificationMethod
        {
            get { return _classificationMethod; }
            set { PropertyChanged.ChangeAndNotify(ref _classificationMethod, value, () => ClassificationMethod); }
        }

        public int NumberOfClasses
        {
            get { return _numberOfClasses; }
            set { PropertyChanged.ChangeAndNotify(ref _numberOfClasses, value, () => NumberOfClasses); }

        }

        public IField Field
        {
            get { return _field; }
            set { PropertyChanged.ChangeAndNotify(ref _field, value, () => Field); }
        }

        public int Bins
        {
            get { return _bins; }
            set { PropertyChanged.ChangeAndNotify(ref _bins, value, () => Bins); }
        }

        /// <summary>
        /// Checks if all required parameters are set.
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            return StartColor != null && EndColor != null && ClassificationMethod != null && Field != null && NumberOfClasses > 0;
        }

    }

}
