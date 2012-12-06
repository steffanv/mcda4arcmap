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
    public class ClassBreaksRendererContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _startColor;
        private Color _endColor;

        private IClassify _classificationMethod;
        private int _numberOfClasses;

        private IField _field;

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

        public IField Field {

            get { return _field;}
            set { PropertyChanged.ChangeAndNotify(ref _field, value, () => Field); }
        }

        /// <summary>
        /// Chechks if all required parameters are set.
        /// </summary>
        /// <returns></returns>
        public bool IsComplete()
        {
            return StartColor != null && EndColor != null && ClassificationMethod != null && Field != null && NumberOfClasses > 0;
        }

    }

    public class BiPolarRendererContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private Color _negativColor;
        private Color _positivColor;
        private Color _neutralColor;

        private IField _field;

        private double _neutralColorPosition;

        public Color NegativColor
        {
            get { return _negativColor; }
            set { PropertyChanged.ChangeAndNotify(ref _negativColor, value, () => NegativColor); }
        }

        public Color PositivColor
        {
            get { return _positivColor; }
            set { PropertyChanged.ChangeAndNotify(ref _positivColor, value, () => PositivColor); }
        }

        public Color NeutralColor
        {
            get { return _neutralColor; }
            set { PropertyChanged.ChangeAndNotify(ref _neutralColor, value, () => NeutralColor); }
        }

        public double NeutralColorPosition
        {
            get { return _neutralColorPosition; }
            set { PropertyChanged.ChangeAndNotify(ref _neutralColorPosition, value, () => NeutralColorPosition); }
        }

        public IField Field
        {

            get { return _field; }
            set { PropertyChanged.ChangeAndNotify(ref _field, value, () => Field); }
        }
    }
}
