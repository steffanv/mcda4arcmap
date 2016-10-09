using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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
