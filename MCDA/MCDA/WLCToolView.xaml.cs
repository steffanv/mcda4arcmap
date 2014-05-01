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
            base.OnClosing(e);
            WLCToolViewModel viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.ClosingCommand.Execute(null);
        }

        private void weightSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            WLCToolViewModel viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();     
        }

        private void weightSliderTextblock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            WLCToolViewModel viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
            viewmodel.Update();
        }
    }
}
