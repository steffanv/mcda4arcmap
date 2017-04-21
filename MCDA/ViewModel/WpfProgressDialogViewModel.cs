using System.Windows.Input;

namespace MCDA.ViewModel
{
    internal sealed class WpfProgressDialogViewModel
    {
        public WpfProgressDialogViewModel()
        {
            Progress = 0;
        }

        public ICommand CancelCommand { get; set; }

        public bool IsCancelButtonEnabled { get; set; }

        public float Progress { get; set; }

        public string ProgressText { get; set; }

        public string Text { get; set; }
    }
}
