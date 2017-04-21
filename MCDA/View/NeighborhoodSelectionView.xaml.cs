using System.Windows;
using MCDA.ViewModel;

namespace MCDA
{
    /// <summary>
    /// Interaction logic for NeighborhoodSelectionView.xaml
    /// </summary>
    public partial class NeighborhoodSelectionView : Window
    {
        public NeighborhoodSelectionView()
        {
            InitializeComponent();

            DataContext = new NeighborhoodSelectionViewModel();

            DataContextChanged += new DependencyPropertyChangedEventHandler(NeighborhoodSelectionView_DataContextChanged);
        }

        void NeighborhoodSelectionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var context = (NeighborhoodSelectionViewModel) DataContext;

            DistanceSlider.Minimum = context.ThresholdMin;
            DistanceSlider.Maximum = context.ThresholdMax;
        }     
    }
}
