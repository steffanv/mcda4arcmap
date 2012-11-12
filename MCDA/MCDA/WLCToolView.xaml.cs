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
using MCDA.ViewModel;

namespace MCDA
{
    /// <summary>
    /// Designer class of the dockable window add-in. It contains WPF user interfaces that
    /// make up the dockable window.
    /// </summary>
    public partial class WLCToolView : UserControl
    {
        public WLCToolView()
        {
            InitializeComponent();

            DataContext = new WLCToolViewModel();
        }

        /// <summary>
        /// Implementation class of the dockable window add-in. It is responsible for 
        /// creating and disposing the user interface class of the dockable window.
        /// </summary>
        public class AddinImpl : ESRI.ArcGIS.Desktop.AddIns.DockableWindow
        {
            private System.Windows.Forms.Integration.ElementHost m_windowUI;

            public AddinImpl()
            {
            }

            protected override IntPtr OnCreateChild()
            {
                m_windowUI = new System.Windows.Forms.Integration.ElementHost();
                m_windowUI.Child = new WLCToolView();
                return m_windowUI.Handle;
            }

            protected override void Dispose(bool disposing)
            {
                if (m_windowUI != null)
                    m_windowUI.Dispose();

                base.Dispose(disposing);
            }
        }

        private bool dragStarted = false;

        private void weightSlider_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            WLCToolViewModel viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
            
            this.dragStarted = false;

        }
        private void weightSlider_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            this.dragStarted = true;
        }

        private void weightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            /*
            if (!dragStarted)
            {
                //work
            }
             * */
        }

        private void benefitCriteriaCheckBox_Click(object sender, RoutedEventArgs e)
        {
            WLCToolViewModel viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
        }

        private void weightSliderTextblock_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            WLCToolViewModel viewmodel = (WLCToolViewModel)DataContext;
            viewmodel.UpdateAllowedEvent();
        }
    }
}
