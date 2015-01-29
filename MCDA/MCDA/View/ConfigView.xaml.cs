using MCDA.ViewModel;
using System.Windows;

namespace MCDA
{  
    public partial class ConfigView : Window
    {
        public ConfigView()
        {
            InitializeComponent();

            var configViewModel = new ConfigViewModel();

            configViewModel.RequestClose += (s, e) => this.Close();

            DataContext = configViewModel;
        }
    }
}
