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

        private void weightSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            LWLCToolViewModel viewmodel = (LWLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent(true);

            viewmodel.Update();
        }

        private void weightSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            LWLCToolViewModel viewmodel = (LWLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent(false);
        }


        private void weightSliderTextblock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            LWLCToolViewModel viewmodel = (LWLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent(true);
            //viewmodel.Update();
        }
    }
}
