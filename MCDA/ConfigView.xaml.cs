using System;
using System.Windows.Controls;
using MCDA.ViewModel;
using System.Windows;

namespace MCDA
{  
    public partial class ConfigView : Window
    {
        public ConfigView()
        {
            InitializeComponent();

            ConfigViewModel configViewModel = new ConfigViewModel();

            configViewModel.RequestClose += (s, e) => this.Close();

            DataContext = configViewModel;
        }
    }
}
