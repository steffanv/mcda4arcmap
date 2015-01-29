using MCDA.Model;
using System.ComponentModel;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    internal sealed class NormalizationSelectionViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public NormalizationStrategy SelectedTransformationStrategy { get; set; }

        public ICommand ApplyCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand OkayCommand { get; set; }
    }
}
