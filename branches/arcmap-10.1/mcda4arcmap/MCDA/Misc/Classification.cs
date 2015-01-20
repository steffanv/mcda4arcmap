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
        /// <summary>
        /// Returns breaks based on the given arguments.
        /// </summary>
        /// <param name="method">The classification method</param>
        /// <param name="featureClass">The featureClass with the data</param>
        /// <param name="field">The field to specify the data</param>
        /// <param name="numberOfClasses">The number of classes</param>
        /// <returns>An array of breaks. NOTE: the first value in the array is the smallest and not part of the breaks. Find more at: http://help.arcgis.com/en/sdk/10.0/arcobjects_net/componenthelp/index.html#/IClassifyGEN_Interface/0042000000m2000000/ </returns>
        public static double[] Classify(IClassifyGEN method, IFeatureClass featureClass, IField field, int numberOfClasses)
        {
            double[] data;
            int[] freq;

            int numberClasses = numberOfClasses;
            
            Histogram(featureClass, field, out data, out freq);

            method.Classify(data, freq, numberOfClasses);

            return method.ClassBreaks as double[];
        }

        public static Tuple<double, int>[] Histogram(Field field, int bins)
        {
            if (bins < 1)
                throw new ArgumentOutOfRangeException("bins cannot be < 1");

           IEnumerable<double> data = field.GetFieldData();

           double min = data.Min();
           double max = data.Max();
           double range = max - min;

           double binSize = range / bins;

           Tuple<double, int>[] histo = Enumerable.Repeat(Tuple.Create<double, int>(0d, 0), bins).ToArray();

           foreach (double currentData in data)
           {
               int index = (int) ((currentData - min) / binSize);

               //max fits into the last bin
               if(index >= histo.Count())
                index--;

               //we add the binMiddle afterwards
               histo[index] = Tuple.Create<double, int>(0d, histo[index].Item2 + 1);
           }

            //add bin middles values
           for (int i = 0; i < bins; i++)
           {
               //double binMiddle = min + (i * binSize) + (binSize / 2);
               double binRight = min + ((i + 1) * binSize);
               histo[i] = Tuple.Create<double, int>(binRight, histo[i].Item2);
           }

           return histo;
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
 
    }
}
