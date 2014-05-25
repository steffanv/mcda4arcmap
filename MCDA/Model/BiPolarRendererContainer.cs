using ESRI.ArcGIS.Geodatabase;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using MCDA.Extensions;

namespace MCDA.Model
{
    internal sealed class BiPolarRendererContainer : INotifyPropertyChanged
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
