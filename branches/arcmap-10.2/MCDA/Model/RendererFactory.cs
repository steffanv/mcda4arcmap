using System;
using System.Collections.Generic;
using System.Linq;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using System.Windows.Media;
using ESRI.ArcGIS.ADF;

namespace MCDA.Model
{
    internal static class RendererFactory
    {
        public static IFeatureRenderer NewSimpleRenderer()
        {
            ISimpleRenderer simpleRenderer = new SimpleRendererClass();

            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            simpleFillSymbol.Outline.Width = 0.4;
            simpleFillSymbol.Color = ToColor(Color.FromRgb(84, 204, 208));

            simpleRenderer.Symbol = simpleFillSymbol as ISymbol;

            return (IFeatureRenderer)simpleRenderer;
        }

        private static IFeatureRenderer NewAllMissingValuesRenderer()
        {
            ISimpleRenderer simpleRenderer = new SimpleRendererClass();

            ISimpleFillSymbol noDataFillSymbol = new SimpleFillSymbolClass();
            noDataFillSymbol.Style = esriSimpleFillStyle.esriSFSDiagonalCross;
            noDataFillSymbol.Outline.Width = .4;
            noDataFillSymbol.Color = ToColor(Color.FromRgb(84, 204, 208));

            simpleRenderer.Symbol = noDataFillSymbol as ISymbol;

            return (IFeatureRenderer)simpleRenderer;
        }

        private static IRgbColor ToColor(Color color)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = color.R;
            rgbColor.Green = color.G;
            rgbColor.Blue = color.B;

            return rgbColor;
        }

        /// <summary>
        /// Returns Simple Renderer if no values can be found
        /// </summary>
        /// <param name="renderContainer"></param>
        /// <returns></returns>
        public static IFeatureRenderer NewUniqueValueRenderer(RendererContainer renderContainer)
        {
            BiPolarRendererContainer biPolarRendererContainer = renderContainer.BiPolarRendererContainer;
            
            IUniqueValueRenderer uniqueValueRenderer = new UniqueValueRendererClass();

            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
            simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            simpleFillSymbol.Outline.Width = 0.4;

            var fieldName = biPolarRendererContainer.Field.Name;
         
            uniqueValueRenderer.FieldCount = 1;
            uniqueValueRenderer.Field[0] = fieldName;

            int fieldIndex;

            ISet<double> setOfFeatures = new HashSet<double>();
            var containsDbNullValue = false;
            using (var comReleaser = new ComReleaser())
            {
                var featureCursor =  renderContainer.Field.Feature.FeatureClass.Search(null, true);
               
                comReleaser.ManageLifetime(featureCursor);

                fieldIndex = featureCursor.Fields.FindField(fieldName);

                IFeature currentFeature;
                
                while ((currentFeature = featureCursor.NextFeature()) != null)
                {
                    var value = currentFeature.Value[fieldIndex];

                    if (Convert.IsDBNull(value))
                    {
                        containsDbNullValue = true;
                        continue;
                    }
                    //TODO datetime is possible???
                    setOfFeatures.Add(Convert.ToDouble(value)); 
                }
            }

            if (containsDbNullValue && setOfFeatures.Count == 0)
            {
                return NewAllMissingValuesRenderer();
            }

            if (setOfFeatures.Count == 0)
            {
                return NewSimpleRenderer();
            }

            IEnumerable<double> orderedSet = setOfFeatures.OrderBy(d => d);

            foreach (var currentClassValue in orderedSet)
             {
                ISimpleFillSymbol pClassSymbol = new SimpleFillSymbolClass();
                pClassSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                pClassSymbol.Outline.Width = 0.4;

                // the format is important, because the normal to string will represent the power as E-05 or something like that
                // the result are mismatches between the renderer value and the column value
                //string classValue = currentClassValue.ToString("N20"); does not really work

                var dirtyTrick = (decimal) currentClassValue;

                var classValue = dirtyTrick.ToString();

                uniqueValueRenderer.AddValue(classValue, fieldName, (ISymbol)pClassSymbol);
                uniqueValueRenderer.set_Label(classValue, classValue);
                uniqueValueRenderer.set_Symbol(classValue, (ISymbol)pClassSymbol);
             }

            //figure out how many colors belong to which side from the neutral color
            var size = uniqueValueRenderer.ValueCount;

            var left = (int) (size * biPolarRendererContainer.NeutralColorPosition);
            var right = size - left;

            // for the case one or both are zero -> create color ramp crashes, the magic value seems to be 2
            if (left < 2)
            {
                left = 2;
            }

            if (right < 2)
            {
                right = 2;
            }

            IAlgorithmicColorRamp firstColorRamp = new AlgorithmicColorRampClass();
           
            firstColorRamp.FromColor = ToColor(biPolarRendererContainer.NegativColor);
            firstColorRamp.ToColor = ToColor(biPolarRendererContainer.NeutralColor);

            firstColorRamp.Size = left;
            bool bOk;

            IAlgorithmicColorRamp secondColorRamp = new AlgorithmicColorRampClass();
           
            secondColorRamp.FromColor = ToColor(biPolarRendererContainer.NeutralColor);
            secondColorRamp.ToColor = ToColor(biPolarRendererContainer.PositivColor);

            secondColorRamp.Size = right;

            firstColorRamp.CreateRamp(out bOk);
            secondColorRamp.CreateRamp(out bOk);

            var firstEnumColors = firstColorRamp.Colors;
            var secondEnumColors = secondColorRamp.Colors;

            firstEnumColors.Reset();
            secondEnumColors.Reset();

            for (var j = 0; j <= uniqueValueRenderer.ValueCount - 1; j++)
            {
                var label = uniqueValueRenderer.Value[j];

                var pSimpleFillColor = (ISimpleFillSymbol)uniqueValueRenderer.Symbol[label];

                    var color = firstEnumColors.Next();

                    //in case the first half colors is "empty" change to the second
                    if (color == null)
                    {
                        firstEnumColors = secondEnumColors;
                        color = firstEnumColors.Next();
                    }

                    pSimpleFillColor.Color = color;
                    
                    uniqueValueRenderer.set_Symbol(label, (ISymbol) pSimpleFillColor);
            }

            //'** If you didn'toolParameter use a predefined color ramp
            //'** in a style, use "Custom" here. Otherwise,
            //'** use the name of the color ramp you selected.
            uniqueValueRenderer.ColorScheme = "Custom";
            ////ITable pTable = pDisplayTable as ITable;
            bool isString = renderContainer.Field.Feature.FeatureClass.Fields.Field[fieldIndex].Type == esriFieldType.esriFieldTypeString;
            uniqueValueRenderer.FieldType[0] = isString;
            //TODO geoFeatureLayer
            var geoFeatureLayer = renderContainer.Field.Feature.FeatureLayer as IGeoFeatureLayer;
            geoFeatureLayer.Renderer = uniqueValueRenderer as IFeatureRenderer;

            return (IFeatureRenderer) uniqueValueRenderer;
        }

        public static IFeatureRenderer NewClassBreaksRenderer(RendererContainer renderContainer)
        {
            var classBreaksRendererContainer = renderContainer.ClassBreaksRendererContainer;

            var classificationResult = Classification.Classify(classBreaksRendererContainer.ClassificationMethod, renderContainer.Field.Feature.FeatureClass, classBreaksRendererContainer.Field, classBreaksRendererContainer.NumberOfClasses);

            IClassBreaksRenderer classBreaksRenderer = new ClassBreaksRendererClass();
            classBreaksRenderer.Field = classBreaksRendererContainer.Field.AliasName;

            if (classificationResult.Count() > 0)
            {
                classBreaksRenderer.BreakCount = classificationResult.Count() - 1;
            }

            if (classificationResult.Count() > 0)
            {
                classBreaksRenderer.MinimumBreak = classificationResult[0];
            }

            //TODO normalization
            //classBreaksRenderer.NormField

            var startColor = classBreaksRendererContainer.StartColor;
            var endColor = classBreaksRendererContainer.EndColor;

            IAlgorithmicColorRamp algorithmicColorRamp = new AlgorithmicColorRampClass();

            algorithmicColorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;

            RgbColor fromColor = new RgbColorClass();
            fromColor.Red = startColor.R;
            fromColor.Green = startColor.G;
            fromColor.Blue = startColor.B;

            algorithmicColorRamp.FromColor = fromColor;

            RgbColor tooColor = new RgbColorClass();
            tooColor.Red = endColor.R;
            tooColor.Green = endColor.G;
            tooColor.Blue = endColor.B;

            algorithmicColorRamp.ToColor = tooColor;

            algorithmicColorRamp.Size = classBreaksRenderer.BreakCount;
            bool bOk;
            algorithmicColorRamp.CreateRamp(out bOk);

            var pEnumColors = algorithmicColorRamp.Colors;

            for (var i = 0; i < classificationResult.Count()-1; i++)
            {
                classBreaksRenderer.Break[i] = classificationResult[i+1];
                //Create simple fill symbol and set color
                ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbolClass();
                simpleFillSymbol.Color = pEnumColors.Next();
                //Add symbol to renderer
                classBreaksRenderer.Symbol[i] = (ISymbol)simpleFillSymbol;
            }

            return (IFeatureRenderer)classBreaksRenderer;
        }
    }

    public enum Renderer
    {
        ClassBreaksRenderer,
        BiPolarRenderer,
        None
    }
}
