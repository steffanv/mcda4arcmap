using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MCDA.CustomControls
{
    /// <summary>
    /// Slider with labels.
    /// </summary>
    public class LabeledSlider : Slider
    {
        /// <summary>
        /// Comma separated list of strings.
        /// </summary>
        public String TickBarText { private get; set; }

        /// <summary>
        /// Draws the TickBarText a each tick.
        /// </summary>
        /// <param name="drawingContext"></param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            String[] labels = Regex.Replace(TickBarText, @"\s+", "").Split(',');
            double range = Maximum - Minimum;
            double x = 0;
            int index = 0;
            for (double i = 0; i <= range; i += TickFrequency)
            {
                var formattedText = new FormattedText(labels.Length > index ? labels[index] : String.Empty, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.Black);
                drawingContext.DrawText(formattedText, new Point((int)x, 25));
                x += ActualWidth / (range / TickFrequency);
                index++;
            }
            base.OnRender(drawingContext);
        }
    }
}
