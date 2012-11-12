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
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Display;
using Xceed.Wpf.Toolkit;
using MCDA.ViewModel;

namespace MCDA
{
  /// <summary>
  /// Designer class of the dockable window add-in. It contains WPF user interfaces that
  /// make up the dockable window.
  /// </summary>
    public partial class VisualizationView : UserControl
    {
        private ColorPicker cp = new ColorPicker();
        private ColorPicker cp2 = new ColorPicker();

        public VisualizationView()
        {
            InitializeComponent();

            DataContext = new VisualizationViewModel();

            InitialiazeColorPicker();
        }

        private void InitialiazeColorPicker()
        {
            cp.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(cp_SelectedColorChanged);
            cp2.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(cp_SelectedColorChanged);

            dockPanel1.Children.Add(cp);
            dockPanel1.Children.Add(cp2);

            colorRampRectangle.Fill = new LinearGradientBrush(cp.SelectedColor, cp2.SelectedColor, 0d);
        }

        void cp_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            colorRampRectangle.Fill = new LinearGradientBrush(cp.SelectedColor, cp2.SelectedColor, 0d);
        }

        //private void CreateSymbology()
        //{
        //    //System.Windows.Forms.Form form = new System.Windows.Forms.Form();

        //    //System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(VisualizationView));

        //    _axSymbologyControl = new ESRI.ArcGIS.Controls.AxSymbologyControl();

        //    _axSymbologyControl.CreateControl();

        //   // symbologyHost.Child = _axSymbologyControl;

        //    //_axSymbologyControl.ContainingControl = this;
        //    _axSymbologyControl.Location = new System.Drawing.Point(8, 16);
        //    _axSymbologyControl.Size = new System.Drawing.Size(272, 265);
        //    //_axSymbologyControl.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("_axSymbologyControl.OcxState")));

        //    symbologyHost.Child = _axSymbologyControl;

        //    //Get the ArcGIS install location
        //    string sInstall = ESRI.ArcGIS.RuntimeManager.ActiveRuntime.Path;

        //    //Load the ESRI.ServerStyle file into the SymbologyControl
        //    _axSymbologyControl.LoadStyleFile(sInstall + @"\Styles\ESRI.ServerStyle");

        //    //Set the style class
        //    _axSymbologyControl.StyleClass = esriSymbologyStyleClass.esriStyleClassColorRamps;

        //    //Select the color ramp item
        //    //_axSymbologyControl.GetStyleClass(_axSymbologyControl.StyleClass).SelectItem(0);

        //    _axSymbologyControl.OnItemSelected += new ESRI.ArcGIS.Controls.ISymbologyControlEvents_Ax_OnItemSelectedEventHandler(_axSymbologyControl_OnItemSelected);
        //}

        //private void _axSymbologyControl_OnItemSelected(object sender, ESRI.ArcGIS.Controls.ISymbologyControlEvents_OnItemSelectedEvent e)
        //{
        //    //Get the selected item
        //    m_styleGalleryItem = (IStyleGalleryItem)e.styleGalleryItem;
        //}

      /// <summary>
      /// Implementation class of the dockable window add-in. It is responsible for 
      /// creating and disposing the user interface class of the dockable window.
      /// </summary>
      public class AddinImpl: ESRI.ArcGIS.Desktop.AddIns.DockableWindow
      {
        private System.Windows.Forms.Integration.ElementHost m_windowUI;

        public AddinImpl()
        {

        }

        protected override IntPtr OnCreateChild()
        {
          m_windowUI = new System.Windows.Forms.Integration.ElementHost();
          m_windowUI.Child = new VisualizationView();
          return m_windowUI.Handle;
        }

        protected override void Dispose(bool disposing)
        {
          if (m_windowUI != null)
            m_windowUI.Dispose();

          base.Dispose(disposing);
        }

      }

    }
}
