using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using System.ComponentModel;
using MCDA.Extensions;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    internal sealed class StandardizationSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
   
        private StandardizationStrategy _selectedTtransformationStrategy;

        private ICommand _applyCommand;
        private ICommand _okayCommand;
        private ICommand _cancelCommand;

        public StandardizationStrategy SelectedTransformationStrategy{

            get{ return _selectedTtransformationStrategy; }
            set { _selectedTtransformationStrategy = value; }
        }

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
    }
}
