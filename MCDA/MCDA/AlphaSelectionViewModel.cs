using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;

namespace MCDA.Model
{
    class AlphaSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private double _alphaValue = 1;
        private int _alphaSliderValue = 4;

        public double AlphaValue
        {
            get { return _alphaValue; }
            set { 
                
                _alphaValue = value;
                AlphaValueToNearestAlphaSliderValue();
            
            }
        }

        public int AlphaSliderValue
        {
            get { return _alphaSliderValue; }
            set {
                
                _alphaSliderValue = value;
                AlphaSliderValueToAlphaValue();
            
            }
        }

        private void AlphaSliderValueToAlphaValue()
        {
            switch (_alphaSliderValue)
            {
                case 1: 
                    _alphaValue = 0.001;
                    break;
                case 2: _alphaValue = 0.1;
                    break;
                case 3: _alphaValue = 0.5;
                    break;
                case 4: _alphaValue = 1;
                    break;
                case 5: _alphaValue = 2;
                    break;
                case 6: _alphaValue = 10;
                    break;
                case 7: _alphaValue = 1000;
                    break;
            }

            PropertyChanged.Notify(() => AlphaValue);
        }

        private void AlphaValueToNearestAlphaSliderValue()
        {
            if(_alphaValue < 000.1)
            {
                _alphaValue = 1;

            }
            else if (_alphaValue < 0.1)
            {
                _alphaValue = 2;

            }
            else if (_alphaValue < 0.5)
            {
                _alphaValue = 3;

            }
            else if (_alphaValue < 1)
            {
                _alphaValue = 4;

            }
            else if (_alphaValue < 2)
            {
                _alphaValue = 5;

            }
            else if (_alphaValue < 10)
            {
                _alphaValue = 6;

            }
            else if (_alphaValue < 1000)
            {
                _alphaValue = 7;

            }

            PropertyChanged.Notify(() => AlphaValue);
        }
        
    }
 
}
