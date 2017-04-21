using System.Linq;
using System.ComponentModel;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    public enum NeighborhoodOptions { KNearestNeighbors, Rook, Queen, Threshold, Automatic }

    internal sealed class NeighborhoodSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public BindingList<int> NumberOfKNearestNeighbors => new BindingList<int>(Enumerable.Range(1, 19).ToList());

        public int SelectedNumberOfKNearestNeighbors { get; set; }

        public int SelectedNumberOfKNearestNeighborsForAutomatic { get; set; }

        public double Threshold { get; set; }

        public double ThresholdMin { get; set; }

        public double ThresholdMax { get; set; }

        public int NumberOfNeighbors { get; set; }

        public NeighborhoodOptions NeighborhoodOption { get; set; }

        public ICommand ApplyCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand OkayCommand { get; set; }
    }   
}
