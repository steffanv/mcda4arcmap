﻿using System;
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
using HistogramControl;

namespace MCDA
{
    public partial class VisualizationView : Window
    {
        private ColorPicker _startColorColorPicker = new ColorPicker();
        private ColorPicker _endColorColorPicker = new ColorPicker();

        private ColorPicker _negativColorPickerForBiPolarRenderer = new ColorPicker();
        private ColorPicker _positivColorPickerForBiPolarRenderer = new ColorPicker();
        private ColorPicker _neutralColorPickerForBipolarRenderer = new ColorPicker();

        private long [] _histogramData;

        HistogramControl.HistogramControl _histogramControl = new HistogramControl.HistogramControl();

        private VisualizationViewModel _viewModel;

        public VisualizationView()
        {
            InitializeComponent();

            InitialiazeColorPicker();

            InitializeHistogramControl();

            DataContext = new VisualizationViewModel();

            _viewModel = (VisualizationViewModel) DataContext;

            //we have to take care of the histogram if the viewmodel changes
            _viewModel.RegisterPropertyHandler(p => p.SelectedClassificationMethod, ViewModelPropertyChanged);
            _viewModel.RegisterPropertyHandler(p => p.SelectedNumberOfClasses, ViewModelPropertyChanged);
            _viewModel.RegisterPropertyHandler(p => p.SelectedResult, ViewModelPropertyChanged);
           
        }

        public long[] HistogramData
        {
            get { return _histogramData; }
            set { _histogramData = value; }
        }

        void ViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            //we use this so far only for the histogram
            if (_viewModel.SelectedResult == null || _viewModel.SelectedResult.RenderContainer == null)
                return;

            double[] data;
            int[] freq;
           
            Classification.Histogram(_viewModel.SelectedResult.RenderContainer.FeatureClass, _viewModel.SelectedResult.Field, out data, out freq);

            _histogramControl.Data = Array.ConvertAll<int, long>(Classification.NormalizeHistogramData(data, freq), Convert.ToInt64);

            double[] classes = Classification.Classify(_viewModel.SelectedClassificationMethod, _viewModel.SelectedResult.RenderContainer.FeatureClass, _viewModel.SelectedResult.Field, _viewModel.SelectedNumberOfClasses);

            _histogramControl.Breaks = Array.ConvertAll<int, long>(Classification.NormalizeBreaks(classes), Convert.ToInt64);   

        }

        private void InitializeHistogramControl()
        {
            histogramGroupBoxDockPanel.Children.Add(_histogramControl);

            Binding _histogramControlDataBinding = new Binding("HistogramData");
            _histogramControl.SetBinding(HistogramControl.HistogramControl.DataProperty, _histogramControlDataBinding);

            Binding _histogramControlBreaksBinding = new Binding("HistogramBreaksData");
            _histogramControl.SetBinding(HistogramControl.HistogramControl.BreaksProperty, _histogramControlBreaksBinding);
        }

        #region Color Picker init

        private void InitialiazeColorPicker()
        {
            //create grey colors for the neutral color
            ObservableCollection<ColorItem> greyScaleColors = new ObservableCollection<ColorItem>();

            for(byte i = 0; i < 254; i+=2){

                greyScaleColors.Add(new ColorItem(Color.FromRgb(i,i,i),"Grey"));
            }

            _neutralColorPickerForBipolarRenderer.AvailableColors = greyScaleColors;
            _neutralColorPickerForBipolarRenderer.ShowAdvancedButton = false;
            _neutralColorPickerForBipolarRenderer.ShowStandardColors = false;
            _neutralColorPickerForBipolarRenderer.ShowRecentColors = false;

            //why doing all this in the code behind page instead of binding the viewmodel?
            //if you try to bind... the esri addin magic cannot find the external dll (this behavior was also described by others)
            //so we have to use this work around
            Binding startColorBinding = new Binding("SelectedStartColor");
            _startColorColorPicker.SetBinding(ColorPicker.SelectedColorProperty, startColorBinding);

            Binding endColorBinding = new Binding("SelectedEndColor");
            _endColorColorPicker.SetBinding(ColorPicker.SelectedColorProperty, endColorBinding);

            _startColorColorPicker.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;
            _endColorColorPicker.VerticalAlignment = System.Windows.VerticalAlignment.Stretch;

            _startColorColorPicker.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(ColorPickerSelectedColorChanged);
            _endColorColorPicker.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(ColorPickerSelectedColorChanged);

            dockPanel1.Children.Add(_startColorColorPicker);
            dockPanel2.Children.Add(_endColorColorPicker);

            colorRampRectangle.Fill = new LinearGradientBrush(_startColorColorPicker.SelectedColor, _endColorColorPicker.SelectedColor, 0d);


            // bi polar stuff
            Binding biPolarNegativColorBinding = new Binding("SelectedBiPolarNegativColor");
            _negativColorPickerForBiPolarRenderer.SetBinding(ColorPicker.SelectedColorProperty, biPolarNegativColorBinding);

            Binding biPolarPositivColorBinding = new Binding("SelectedBiPolarPositivColor");
            _positivColorPickerForBiPolarRenderer.SetBinding(ColorPicker.SelectedColorProperty, biPolarPositivColorBinding);

            Binding biPolarNeutralColorBinding = new Binding("SelectedBiPolarNeutralColor");
            _neutralColorPickerForBipolarRenderer.SetBinding(ColorPicker.SelectedColorProperty, biPolarNeutralColorBinding);

            biPolarNegativColorDockPanel.Children.Add(_negativColorPickerForBiPolarRenderer);
            biPolarPositivColorDockPanel.Children.Add(_positivColorPickerForBiPolarRenderer);
            biPolarNeutralColorDockPanel.Children.Add(_neutralColorPickerForBipolarRenderer);
            
            LinearGradientBrush biPolarColorScaleRectagleBrush = new LinearGradientBrush(_negativColorPickerForBiPolarRenderer.SelectedColor, _positivColorPickerForBiPolarRenderer.SelectedColor, 0d);
            biPolarColorScaleRectagleBrush.GradientStops.Add(new GradientStop(_neutralColorPickerForBipolarRenderer.SelectedColor, 0.5d));

            biPolarColorScaleRectangle.Fill = biPolarColorScaleRectagleBrush;

            _negativColorPickerForBiPolarRenderer.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(ColorPickerForBiPolarRendererSelectedColorChanged);
            _positivColorPickerForBiPolarRenderer.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(ColorPickerForBiPolarRendererSelectedColorChanged);
            _neutralColorPickerForBipolarRenderer.SelectedColorChanged += new RoutedPropertyChangedEventHandler<Color>(ColorPickerForBiPolarRendererSelectedColorChanged);

            biPolarColorSlider.ValueChanged += new RoutedPropertyChangedEventHandler<double>(BiPolarColorSliderValueChanged);
        }

        #endregion

        private void RedefineBiPolarLinearGradient()
        {
            LinearGradientBrush biPolarColorScaleRectagleBrush = new LinearGradientBrush(_negativColorPickerForBiPolarRenderer.SelectedColor, _positivColorPickerForBiPolarRenderer.SelectedColor, 0d);

            biPolarColorScaleRectagleBrush.GradientStops.Add(new GradientStop(_neutralColorPickerForBipolarRenderer.SelectedColor, biPolarColorSlider.Value / 100));

            biPolarColorScaleRectangle.Fill = biPolarColorScaleRectagleBrush;
        }

        private void BiPolarColorSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedefineBiPolarLinearGradient();
        }

        private void ColorPickerForBiPolarRendererSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            RedefineBiPolarLinearGradient();
        }

        private void ColorPickerSelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            colorRampRectangle.Fill = new LinearGradientBrush(_startColorColorPicker.SelectedColor, _endColorColorPicker.SelectedColor, 0d);
        }

        private void biPolarColorSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            VisualizationViewModel viewmodel = (VisualizationViewModel)DataContext;
            viewmodel.BiPolarRendererValuesChanged();
        }

        private void SwitchList(object sender, EventArgs e)
        {
            fc.Flip();
        }
    }
}
