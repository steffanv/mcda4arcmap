using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Input;
using MCDA.Model;

namespace MCDA.ViewModel
{
    public enum NeighborhoodOptions { KNearestNeighbors, Rook, Queen, Threshold, Automatic }

    class NeighborhoodSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private NeighborhoodOptions _neigboorhoodOption;

        private ICommand _applyCommand;
        private ICommand _okayCommand;
        private ICommand _cancelCommand;

        public BindingList<int> NumberOfKNearestNeighbors
        {
            get { return new BindingList<int>(Enumerable.Range(1, 19).ToList()); }
        }

        public int SelectedNumberOfKNearestNeighbors { get; set; }

        public int SelectedNumberOfKNearestNeighborsForAutomatic { get; set; }

        public double Threshold { get; set; }

        public double ThresholdMin { get; set; }

        public double ThresholdMax { get; set; }

        public NeighborhoodOptions NeighborhoodOption
        {
            get { return _neigboorhoodOption; }
            set { _neigboorhoodOption = value; }
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
