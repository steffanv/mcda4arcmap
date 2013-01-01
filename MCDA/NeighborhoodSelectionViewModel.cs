using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace MCDA.ViewModel
{
    public enum NeighborhoodOptions { KNearestNeighbors, Contiguity, Threshold }

    class NeighborhoodSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private NeighborhoodOptions _neigboorhoodOption;

        public BindingList<int> NumberOfKNearestNeighbors
        {
            get { return new BindingList<int>(Enumerable.Range(2, 19).ToList()); }
        }

        public int SelectedNumberOfKNearestNeighbors { get; set; }

        public NeighborhoodOptions NeighborhoodOption
        {
            get { return _neigboorhoodOption; }
            set { _neigboorhoodOption = value; }
        }
    }

    
}
