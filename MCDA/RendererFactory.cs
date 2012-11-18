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
        public static IClassBreaksRenderer newClassBreaksRenderer(ClassBreaksRendererContainer classBreaksRendererContainer, MCDAWorkspaceContainer MCDAWorkspaceContainer)
        {
            double[] classificationResult = Classification.Classify(classBreaksRendererContainer.ClassificationMethod, MCDAWorkspaceContainer.FeatureClass, classBreaksRendererContainer.Field, classBreaksRendererContainer.NumberOfClasses);

            IClassBreaksRenderer classBreaksRenderer = new ClassBreaksRendererClass();
            classBreaksRenderer.Field = classBreaksRendererContainer.Field.AliasName;
            classBreaksRenderer.BreakCount = classificationResult.Count();
            classBreaksRenderer.MinimumBreak = classificationResult.Min();

            Color startColor = classBreaksRendererContainer.StartColor;
            Color endColor = classBreaksRendererContainer.EndColor;

            IAlgorithmicColorRamp randomColorRamp = new AlgorithmicColorRampClass();
            //Create the color ramp for the symbols in the renderer.
            randomColorRamp.Algorithm = esriColorRampAlgorithm.esriHSVAlgorithm;

            RgbColor fromColor  = new RgbColorClass(); 
            fromColor.Red = startColor.R;
            fromColor.Green = startColor.G;
            fromColor.Blue = startColor.B;

            randomColorRamp.FromColor = fromColor;

            RgbColor toColor = new RgbColorClass();
            toColor.Red = endColor.R;
            toColor.Green = endColor.G;
            toColor.Blue = endColor.B;
            
            randomColorRamp.ToColor = toColor;

            //randomColorRamp.MinSaturation = (int) (System.Drawing.Color.FromArgb(startColor.R, startColor.G, startColor.B).GetSaturation() * 100f);
            //randomColorRamp.MaxSaturation = (int)(System.Drawing.Color.FromArgb(endColor.R, endColor.G, endColor.B).GetSaturation() * 100f);
            //randomColorRamp.MinValue = 0;
            //randomColorRamp.MaxValue = 100;
            //randomColorRamp.StartHue = (int)System.Drawing.Color.FromArgb(startColor.R, startColor.G, startColor.B).GetHue();
            //randomColorRamp.EndHue = (int)System.Drawing.Color.FromArgb(endColor.R, endColor.G, endColor.B).GetHue();

            randomColorRamp.Size = classBreaksRenderer.BreakCount;
            bool bOK;
            randomColorRamp.CreateRamp(out bOK);

            IEnumColors pEnumColors = randomColorRamp.Colors;

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

            return classBreaksRenderer;
        }     
    }
}
