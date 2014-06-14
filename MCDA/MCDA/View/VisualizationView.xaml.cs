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
using MCDA.Model;
using System.Collections.ObjectModel;
using MCDA.Extensions;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Xps;
using OxyPlot.Series;

namespace MCDA
{
    public partial class VisualizationView : Window
    {

        private VisualizationViewModel visualizationViewModel = new VisualizationViewModel();

        public VisualizationView()
        {
            //WORKAROUND to load the assemblys before XAML comes into play
            new OxyPlot.Wpf.BarSeries();
            new OxyPlot.Xps.XpsExporter();
            new OxyPlot.Series.BarItem();

            InitializeComponent();

            DataContext = visualizationViewModel;

            //To make sure that the neutral color consists only of gray scale values
            ObservableCollection<ColorItem> greyScaleColors = new ObservableCollection<ColorItem>();

            for (byte i = 0; i < 254; i += 2)
                greyScaleColors.Add(new ColorItem(Color.FromRgb(i, i, i), "Grey"));

            BiPolarRendererNeutralColor.AvailableColors = greyScaleColors;

            visualizationViewModel.RegisterPropertyHandler(p => p.HistogramBreaks, HistogramBreaksChanged);
            visualizationViewModel.RegisterPropertyHandler(p => p.HistogramBreaks, NumberOfClassesChanged);

        }

        private void HistogramBreaksChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Histogram.Annotations.Clear();

            foreach (var item in visualizationViewModel.HistogramBreaks)
            {
                double histogramBreak = item.Item1;

                LineAnnotation annotation = new LineAnnotation();
                annotation.X = histogramBreak;
                annotation.Type = OxyPlot.Annotations.LineAnnotationType.Vertical;
                annotation.Color = Colors.Red;
                annotation.Text = item.Item2;

                Histogram.Annotations.Add(annotation);

                Histogram.InvalidatePlot(true);
            }
        }

        private void NumberOfClassesChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!BreaksPopup.IsOpen)
                return;

            if (visualizationViewModel.HistogramBreaks == null)
                BreaksPopupTextBlock.Text = "Not available.";

            BreaksPopupTextBlock.Text = string.Join(Environment.NewLine, visualizationViewModel.HistogramBreaks.Select(t => t.Item2));
        }

        private void SwitchList(object sender, EventArgs e)
        {
            fc.Flip();
        }

        private void TreeViewSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            Field selectedField = e.NewValue as Field;

            if (selectedField != null)
            {
                VisualizationViewModel visualizationViewModel = DataContext as VisualizationViewModel;

                visualizationViewModel.SelectedFieldToRender = selectedField.RenderContainer;

            }

        }

        /// <summary>
        /// Make sure that certain fields can not be selected/ focused as they are not suitable for MCDA.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeViewLoaded(object sender, RoutedEventArgs e)
        {
            var treeView = e.Source as TreeView;

            SetFocusable(treeView);
        }

        private static void SetFocusable(ItemsControl parentContainer)
        {
            foreach (Object item in parentContainer.Items)
            {
                TreeViewItem currentContainer = parentContainer.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

                if (currentContainer != null && currentContainer.Items.Count > 0)
                    SetFocusable(currentContainer);

                Field field = currentContainer.Header as Field;

                if (field != null)
                {

                    if (!field.IsSuitableForMCDA)
                        currentContainer.Focusable = false;
                    //if (!field.IsToolField)
                    //    currentContainer.Visibility = Visibility.Collapsed;
                    if (field.IsToolField)
                        currentContainer.Background = new SolidColorBrush(Colors.LightGreen);
                }

            }
        }

        private void ToggleButton_Checked(object sender, RoutedEventArgs e)
        {
            BreaksPopup.IsOpen = true;

            if (visualizationViewModel.HistogramBreaks == null)
                BreaksPopupTextBlock.Text = "Not available.";

            else
                BreaksPopupTextBlock.Text = string.Join(Environment.NewLine, visualizationViewModel.HistogramBreaks.Select(t => t.Item2));
        }

        private void ToggleButton_Unchecked(object sender, RoutedEventArgs e)
        {
            BreaksPopup.IsOpen = false;          
        }
    }
}
