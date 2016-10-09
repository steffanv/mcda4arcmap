using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MCDA.ViewModel;
using MCDA.Model;

namespace MCDA
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains WPF user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class WLCToolView : Window
    {

        public WLCToolView()
        {
            InitializeComponent();

            DataContext = new WLCToolViewModel();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
            var userResult = msgBox.DoModal("Closing", "When closing the tool you will lose your settings.", "Close", "Abort", ArcMap.Application.hWnd);

            if (userResult)
            {
                base.OnClosing(e);
                var viewmodel = (WLCToolViewModel)DataContext;
                viewmodel.ClosingCommand.Execute(null);
            }
            else
            {
                e.Cancel = true;
            }

        }

        private void WeightSliderDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            var viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
        }

        private void WeightSliderTextblockKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                var weightSliderTextblock = sender as TextBox;
                var bindingExpression = weightSliderTextblock.GetBindingExpression(TextBox.TextProperty);


                IToolParameter toolParameter = bindingExpression.DataItem as ToolParameter;

                double w;
                if (Double.TryParse(weightSliderTextblock.Text, out w))
                {
                    if (toolParameter.AcceptableWeightRange.ContainsValue(w))
                    {
                        bindingExpression.UpdateSource();
                    }
                    else
                    {
                        bindingExpression.UpdateTarget();
                    }
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
                var toolTip = new System.Windows.Controls.ToolTip { Content = "Please enter a value in the following range: " + toolParameter.AcceptableWeightRange.ToString(3) + "." };
                weightSliderTextblock.ToolTip = toolTip;
            }
        }
    }
}
