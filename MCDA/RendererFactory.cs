using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Display;
using System.Windows.Media;
using System.ComponentModel;
using MCDA.Extensions;

namespace MCDA.Model
{
    public static class RendererFactory
    {
        public static IFeatureRenderer NewSimpleRenderer()
        {
            ISimpleRenderer simpleRenderer = new SimpleRendererClass();

            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            pSimpleFillSymbol.Outline.Width = 0.4;
            pSimpleFillSymbol.Color = ToColor(Color.FromRgb(64, 224, 208));

            simpleRenderer.Symbol = pSimpleFillSymbol as ISymbol;

            return (IFeatureRenderer) simpleRenderer;
        }

        private static IRgbColor ToColor(Color color)
        {
            IRgbColor rgbColor = new RgbColorClass();
            rgbColor.Red = color.R;
            rgbColor.Green = color.G;
            rgbColor.Blue = color.B;

            return rgbColor;
        }

        public static IFeatureRenderer NewUniqueValueRenderer(MCDAWorkspaceContainer mcdaWorkspaceContainer)
        {
            BiPolarRendererContainer biPolarRendererContainer = mcdaWorkspaceContainer.BiPolarRendererContainer;
            //Make the renderer.
            IUniqueValueRenderer uniqueValueRenderer = new UniqueValueRendererClass();

            ISimpleFillSymbol pSimpleFillSymbol = new SimpleFillSymbolClass();
            pSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
            pSimpleFillSymbol.Outline.Width = 0.4;

            string fieldName = biPolarRendererContainer.Field.Name;

            //These properties should be set prior to adding values.
            uniqueValueRenderer.FieldCount = 1;
            uniqueValueRenderer.set_Field(0, fieldName);
            uniqueValueRenderer.DefaultSymbol = pSimpleFillSymbol as ISymbol;
            uniqueValueRenderer.UseDefaultSymbol = true;

            IGeoFeatureLayer pGeoFeatureLayer = mcdaWorkspaceContainer.FeatureLayer as IGeoFeatureLayer;

            IDisplayTable pDisplayTable = pGeoFeatureLayer as IDisplayTable;
            IFeatureCursor pFeatureCursor = pDisplayTable.SearchDisplayTable(null, false) as IFeatureCursor;
            IFeature pFeature = pFeatureCursor.NextFeature();

            IList<double> listOfFeatures = new List<double>();

            int fieldIndex;
            IFields pFields = pFeatureCursor.Fields;
            fieldIndex = pFields.FindField(fieldName);

            while (pFeature != null)
            {
                listOfFeatures.Add(Convert.ToDouble(pFeature.get_Value(fieldIndex)));

                pFeature = pFeatureCursor.NextFeature();
            }

            bool ValFound;

            IEnumerable<double> sortedFeatures = listOfFeatures.OrderBy(d => d);

            //while (pFeature != null)
            foreach(double currentClassValue in sortedFeatures)
            {
                ISimpleFillSymbol pClassSymbol = new SimpleFillSymbolClass();
                pClassSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                pClassSymbol.Outline.Width = 0.4;

                string classValue;
                classValue = Convert.ToString(currentClassValue);

                //Test to see if this value was added
                //to the renderer. If not, add it.
                ValFound = false;
                for (int i = 0; i <= uniqueValueRenderer.ValueCount - 1; i++)
                {
                    if (uniqueValueRenderer.get_Value(i) == classValue)
                    {
                        ValFound = true;
                        break; //Exit the loop if the value was found.
                    }
                }
                //If the value was not found, it is new and it will be added.
                if (ValFound == false)
                {
                    uniqueValueRenderer.AddValue(classValue, fieldName, pClassSymbol as ISymbol);
                    uniqueValueRenderer.set_Label(classValue, classValue);
                    uniqueValueRenderer.set_Symbol(classValue, pClassSymbol as ISymbol);
                }
                //pFeature = pFeatureCursor.NextFeature();
            }

            //MultiPartColorRamp multiPartColorRamp = new MultiPartColorRampClass()

            //figure out how many colors belong to which side from the neutral color
            int size = uniqueValueRenderer.ValueCount;

            int left = (int) (size * (biPolarRendererContainer.NeutralColorPosition / 100));
            int right = size - left;

            IAlgorithmicColorRamp firstColorRamp = new AlgorithmicColorRampClass();
           
            //firstColorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;
            firstColorRamp.FromColor = ToColor(biPolarRendererContainer.NegativColor);
            firstColorRamp.ToColor = ToColor(biPolarRendererContainer.NeutralColor);

            firstColorRamp.Size = left;
            bool bOK;
            firstColorRamp.CreateRamp(out bOK);

            IAlgorithmicColorRamp secondColorRamp = new AlgorithmicColorRampClass();
           
            secondColorRamp.FromColor = ToColor(biPolarRendererContainer.NeutralColor);
            secondColorRamp.ToColor = ToColor(biPolarRendererContainer.PositivColor);

            secondColorRamp.Size = right;
           
            secondColorRamp.CreateRamp(out bOK);

            IEnumColors firstEnumColors = firstColorRamp.Colors;
            IEnumColors secondEnumColors = secondColorRamp.Colors;

            firstEnumColors.Reset();
            secondEnumColors.Reset();

            for (int j = 0; j <= uniqueValueRenderer.ValueCount - 1; j++)
            {
                string xv = uniqueValueRenderer.get_Value(j);

                if (xv != String.Empty)
                {
                    ISimpleFillSymbol pSimpleFillColor = uniqueValueRenderer.get_Symbol(xv) as ISimpleFillSymbol;

                    IColor color = firstEnumColors.Next();

                    //in case the first half colors is "empty" change to the second
                    if (color == null)
                    {
                        firstEnumColors = secondEnumColors;
                        color = firstEnumColors.Next();
                    }

                    pSimpleFillColor.Color = color;
                    
                    uniqueValueRenderer.set_Symbol(xv, pSimpleFillColor as ISymbol);

                }
            }

            //'** If you didn't use a predefined color ramp
            //'** in a style, use "Custom" here. Otherwise,
            //'** use the name of the color ramp you selected.
            uniqueValueRenderer.ColorScheme = "Custom";
            ITable pTable = pDisplayTable as ITable;
            bool isString = pTable.Fields.get_Field(fieldIndex).Type == esriFieldType.esriFieldTypeString;
            uniqueValueRenderer.set_FieldType(0, isString);
            pGeoFeatureLayer.Renderer = uniqueValueRenderer as IFeatureRenderer;

            //This makes the layer properties symbology tab
            //show the correct interface.
            IUID pUID = new UIDClass();
            pUID.Value = "{683C994E-A17B-11D1-8816-080009EC732A}";
            pGeoFeatureLayer.RendererPropertyPageClassID = pUID as UIDClass;

            return (IFeatureRenderer) uniqueValueRenderer;
        }


        public static IFeatureRenderer NewClassBreaksRenderer(MCDAWorkspaceContainer MCDAWorkspaceContainer)
        {
            ClassBreaksRendererContainer classBreaksRendererContainer = MCDAWorkspaceContainer.ClassBreaksRendererContainer;

            double[] classificationResult = Classification.Classify(classBreaksRendererContainer.ClassificationMethod, MCDAWorkspaceContainer.FeatureClass, classBreaksRendererContainer.Field, classBreaksRendererContainer.NumberOfClasses);

            IClassBreaksRenderer classBreaksRenderer = new ClassBreaksRendererClass();
            classBreaksRenderer.Field = classBreaksRendererContainer.Field.AliasName;
            classBreaksRenderer.BreakCount = classificationResult.Count();
            classBreaksRenderer.MinimumBreak = classificationResult.Min();

            Color startColor = classBreaksRendererContainer.StartColor;
            Color endColor = classBreaksRendererContainer.EndColor;

            IAlgorithmicColorRamp algorithmicColorRamp = new AlgorithmicColorRampClass();
            //Create the color ramp for the symbols in the renderer.
            algorithmicColorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;

            RgbColor fromColor  = new RgbColorClass(); 
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
            bool bOK;
            algorithmicColorRamp.CreateRamp(out bOK);

            IEnumColors pEnumColors = algorithmicColorRamp.Colors;

            ISimpleFillSymbol simpleFillSymbol;
            //Loop through each class break
            for (int i = 0; i <= classBreaksRenderer.BreakCount - 1; i++)
            {
                classBreaksRenderer.set_Break(i, classificationResult[i]);
                //Create simple fill symbol and set color
                simpleFillSymbol = new SimpleFillSymbolClass();
                simpleFillSymbol.Color = pEnumColors.Next();
                //Add symbol to renderer
                classBreaksRenderer.set_Symbol(i, (ISymbol)simpleFillSymbol);
            }

            return (IFeatureRenderer) classBreaksRenderer;
        }     
    }

    public enum Renderer
    {
        ClassBreaksRenderer,
        BiPolarRenderer,
        None
    }
}
