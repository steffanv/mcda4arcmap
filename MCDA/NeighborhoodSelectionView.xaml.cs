using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
        }
    }
}
