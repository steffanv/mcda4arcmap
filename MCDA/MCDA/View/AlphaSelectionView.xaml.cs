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
    public partial class AlphaSelectionView : Window
    {
        public AlphaSelectionView()
        {
            InitializeComponent();

            DataContext = new AlphaSelectionViewModel();
        }

        private void AlphaTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var alphaTextBox = sender as TextBox;
                var bindingExpression = alphaTextBox.GetBindingExpression(TextBox.TextProperty);

                double w;
                if (Double.TryParse(alphaTextBox.Text, out w))
                {
                    bindingExpression.UpdateSource();
                }
            }
        }
    }
}
