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
using MCDA.Extensions;
using OxyPlot;
using OxyPlot.Wpf;
using OxyPlot.Xps;
using OxyPlot.Series;

namespace MCDA
{
    public partial class VisualizationView : Window
    {
        public VisualizationView()
        {

            //WORKAROUND to load the assemblys before XAML comes into play
            new OxyPlot.Wpf.BarSeries();
            new OxyPlot.Xps.XpsExporter();
            new OxyPlot.Series.BarItem();

            InitializeComponent();

            DataContext = new VisualizationViewModel();

            //To make sure that the neutral color consists only of gray scale values
            ObservableCollection<ColorItem> greyScaleColors = new ObservableCollection<ColorItem>();

            for (byte i = 0; i < 254; i += 2)
                greyScaleColors.Add(new ColorItem(Color.FromRgb(i, i, i), "Grey"));

            BiPolarRendererNeutralColor.AvailableColors = greyScaleColors;

            
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
                if (!selectedField.IsSuitableForMCDA)
                {
                    //TreeViewFeatureName.SelectedItem = e.OldValue;
                }
                else
                {
                    VisualizationViewModel visualizationViewModel = DataContext as VisualizationViewModel;

                    visualizationViewModel.SelectedFieldToRender = selectedField.RenderContainer;
                }
            }

        }

    }
}
