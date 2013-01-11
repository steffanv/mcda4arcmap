using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Extensions;
using System.Windows.Input;

namespace MCDA.Model
{
    class AlphaSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private ICommand _applyCommand;
        private ICommand _okayCommand;
        private ICommand _cancelCommand;

        private double _alphaValue = 1;
        private int _alphaSliderValue = 4;

        public ICommand ApplyCommand
        {
            get { return _applyCommand; }
            set { _applyCommand = value; }
        }

        public ICommand CancelCommand
        {
            get { return _cancelCommand; }
            set { _cancelCommand = value; }
        }

        public ICommand OkayCommand
        {
            get { return _okayCommand; }
            set { _okayCommand = value; }
        }

        public double Alpha
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
                case 1: _alphaValue = 0.001;
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

            PropertyChanged.Notify(() => Alpha);
        }

        private void AlphaValueToNearestAlphaSliderValue()
        {
            if(_alphaValue <= 0.001)
            {
                _alphaSliderValue = 1;

            }
            else if (_alphaValue <= 0.1)
            {
                _alphaSliderValue = 2;

            }
            else if (_alphaValue <= 0.5)
            {
                _alphaSliderValue = 3;

            }
            else if (_alphaValue <= 1)
            {
                _alphaSliderValue = 4;

            }
            else if (_alphaValue <= 2)
            {
                _alphaSliderValue = 5;

            }
            else if (_alphaValue <= 10)
            {
                _alphaSliderValue = 6;

            }
            else if (_alphaValue <= 1000)
            {
                _alphaSliderValue = 7;

            }

            PropertyChanged.Notify(() => AlphaSliderValue);
        }
        
    }
 
}
