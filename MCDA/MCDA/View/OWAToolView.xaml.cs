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
    public partial class OWAToolView : Window
    {
        public OWAToolView()
        {
            InitializeComponent();

            DataContext = new OWAToolViewModel();
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
            bool userResult = msgBox.DoModal("Closing", "When closing the tool you will lose your settings.", "Close", "Abort", ArcMap.Application.hWnd);

            if (userResult)
            {
                base.OnClosing(e);
                OWAToolViewModel viewmodel = (OWAToolViewModel)DataContext;
                viewmodel.ClosingCommand.Execute(null);
            }
            else
            {
                e.Cancel = true;
            }

        }

        private void weightSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            OWAToolViewModel viewmodel = (OWAToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
        }

        private void weightSliderTextblock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            OWAToolViewModel viewmodel = (OWAToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
            viewmodel.Update();
        }
       
    }
}
