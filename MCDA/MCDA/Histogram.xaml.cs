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
using MCDA.Model;

namespace MCDA.CustomControls
{
    /// <summary>
    /// Interaction logic for Histogram.xaml
    /// </summary>
    public partial class Histogram : ContentControl
    {
        private double[] _data;
        private int[] _freq;
        private double[] _breaks;

        public Histogram()
        {
            InitializeComponent();
        }
        
        public double[] Data {

            get { return _data; }
            set { _data = value; }
        }

        public int[] Frequency {

            get { return _freq; }
            set { _freq = value; }
        }

        public double[] Breaks{

             get {return _breaks;}
             set { _breaks = value;

             DrawBreaks();
             
             }
        }

        private void DrawBreaks()
        {
            //TODO braks can contains no elements -> no MAX possible
            if (_breaks == null)
                return;

            double maxDataValue = _breaks.Max();

            int size = 100;

            int[] values = new int[size + 1];

            for (int i = 0; i < _breaks.Length; i++)
            {
                if (double.IsNaN(_breaks[i]))
                    continue;

                int index = (int)Math.Round(_breaks[i] * (size / maxDataValue), 0);
                values[index] = 15;
            }

            int max = values.Max();

            PointCollection points = new PointCollection();
            // first point (lower-left corner)
            points.Add(new Point(0, max));
            // middle points
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > 0)
                    points.Add(new Point(i, max - values[i]));

                else
                    points.Add(new Point(i, max));

            }
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, max));

            breaksPolygon.Points = points;
        }

        public void DrawHistogram()
        {
            if (_data == null || _freq == null || _data.Count() == 0)
                return;

            double maxDataValue = _data.Max();

            int size = 100;

            int[] values = new int[size+1];

            for (int i = 0; i < _data.Length; i++)
            {
                if(Double.IsNaN(_data[i]))
                    continue;
                //hier kracht es auch noch -> index
                int index = (int) Math.Round(_data[i] * (size / maxDataValue),0);
                values[index] += _freq[i];
            }

            //values = Classification.SmoothHistogram(values);

            int max = values.Max();

            PointCollection points = new PointCollection();
            // first point (lower-left corner)
            points.Add(new Point(0, max));
            // middle points
            for (int i = 0; i < values.Length; i++){
 
            points.Add(new Point(i, max - values[i]));

            }
            // last point (lower-right corner)
            points.Add(new Point(values.Length - 1, max));

            histogramPolygon.Points = points;
        }
    }
}
