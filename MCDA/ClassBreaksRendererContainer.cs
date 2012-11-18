﻿using System;
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

        //public MCDAWorkspaceContainer MCDAWorkspaceContainer { get; set; }
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
}
