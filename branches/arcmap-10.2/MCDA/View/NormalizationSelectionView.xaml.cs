using System.Windows;
using MCDA.ViewModel;

namespace MCDA
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains WPF user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class NormalizationSelectionView : Window
    {
        public NormalizationSelectionView()
        {
            InitializeComponent();

            DataContext = new NormalizationSelectionViewModel();

        }     
    }
}
