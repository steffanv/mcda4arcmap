using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace MCDA.Model
{
    class Classification
    {
        public static double[] Classify(IClassify method, IFeatureClass featureClass, IField field, int numberOfClasses)
        {
            numberOfClasses--;

            double[] data;
            int[] freq;

            Histogram(featureClass, field, out data, out freq);

            method.SetHistogramData(data, freq);

            method.Classify(numberOfClasses);

            return method.ClassBreaks as double[];
        }

        public static void Histogram(IFeatureClass featureClass, IField field, out double[] data, out int[] freq)
        {
            ITableHistogram tableHistorgram = new BasicTableHistogramClass();
            IBasicHistogram basicHistogram = tableHistorgram as IBasicHistogram;

            tableHistorgram.Table = (ITable)featureClass;
            tableHistorgram.Field = field.Name;

            object dataFrequency, dataValues;

            basicHistogram.GetHistogram(out dataValues, out dataFrequency);

            data = dataValues as double[];
            freq = dataFrequency as int[];
        }

        //http://trompelecode.com/2012/04/how-to-create-an-image-histogram-using-csharp-and-wpf/
        public static int[] SmoothHistogram(int[] originalValues) 
        { 
            int[] smoothedValues = new int[originalValues.Length]; 
            double[] mask = new double[] { 0.25, 0.5, 0.25 };

            for (int bin = 1; bin < originalValues.Length - 1; bin++) {
 
                double smoothedValue = 0; 
                for (int i = 0; i < mask.Length; i++)
                { 
                    smoothedValue += originalValues[bin - 1 + i] * mask[i]; 
                } 
                smoothedValues[bin] = (int)smoothedValue;
            }

            return smoothedValues; }
    }
}
