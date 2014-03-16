using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;

namespace MCDA.Model
{
    internal static class Classification
    {
        public static double[] Classify(IClassify method, IFeatureClass featureClass, IField field, int numberOfClasses)
        {
            //TODO check ESRI API
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

        public static int[] NormalizeHistogramData(double[] data, int[] freq)
        {
            if (data == null || freq == null || data.Length <= 0 || freq.Length <= 0)
                return new int [0];

            int[] normalizedValues = new int[100];

            double max = data.Max();
            double min = data.Min();

            if (max - min == 0)
                return new int[0];

            double divisor = max - min;

            for(int i = 0; i < data.Length; i++){

                int position = (int) Math.Round(((data[i] - min) / divisor)*99, 0);
                normalizedValues[position] += freq[i];
            }

            return normalizedValues;
        }

        public static int[] NormalizeBreaks(double[] breaks)
        {
            if (breaks == null || breaks.Length <= 0)
                return new int[0];

            int[] normalizedValues = new int[breaks.Length];

            double max = breaks.Max();
            double min = breaks.Min();

            if (max - min == 0)
                return new int[0];

            double divisor = max - min;

            for (int i = 0; i < breaks.Length; i++)
            {
                normalizedValues[i] = (int)Math.Round(((breaks[i] - min) / divisor) * 99, 0);
            }

            return normalizedValues;
        }
       
    }
}
