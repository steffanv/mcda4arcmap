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
using MCDA.Model;

namespace MCDA
{
    /// <summary>
    /// Interaction logic for LWLCToolView.xaml
    /// </summary>
    public partial class LWLCToolView : Window
    {
        public LWLCToolView()
        {
            InitializeComponent();

            DataContext = new LWLCToolViewModel();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
            bool userResult = msgBox.DoModal("Closing", "When closing the tool you will lose your settings.", "Close", "Abort", ArcMap.Application.hWnd);

            if (userResult)
            {
                base.OnClosing(e);
                LWLCToolViewModel viewmodel = (LWLCToolViewModel)DataContext;
                viewmodel.ClosingCommand.Execute(null);
            }
            else
            {
                e.Cancel = true;
            }

        }

        private void WeightSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            LWLCToolViewModel viewmodel = (LWLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent(true);

            viewmodel.Update();
        }

        //private void weightSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        //{
        //    LWLCToolViewModel viewmodel = (LWLCToolViewModel)DataContext;
        //    viewmodel.UpdateAllowedEvent(false);
        //}


        private void WeightSliderTextblockKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                TextBox weightSliderTextblock = sender as TextBox;
                var bindingExpression = weightSliderTextblock.GetBindingExpression(TextBox.TextProperty);


                IToolParameter toolParameter = bindingExpression.DataItem as ToolParameter;

                double w;
                if (Double.TryParse(weightSliderTextblock.Text, out w))
                {
                    if (toolParameter.AcceptableWeightRange.ContainsValue(w))
                        bindingExpression.UpdateSource();
                    else
                        bindingExpression.UpdateTarget();
                }
            }
        }

        private void WeightSliderTextblockToolTipOpening(object sender, ToolTipEventArgs e)
        {
            var weightSliderTextblock = sender as TextBox;

            var bindingExpression = weightSliderTextblock.GetBindingExpression(TextBox.TextProperty);

            var toolParameter = bindingExpression.DataItem as ToolParameter;

            double w;
            if (Double.TryParse(weightSliderTextblock.Text, out w))
            {
                ToolTip toolTip = new System.Windows.Controls.ToolTip { Content = "Please enter a value in the following range: " + toolParameter.AcceptableWeightRange.ToString(3) + "." };
                weightSliderTextblock.ToolTip = toolTip;
            }
        }
    }
}
