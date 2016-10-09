using System;
using System.Windows.Controls;

namespace MCDA.Model
{
    internal sealed class WeightValidationRule : ValidationRule
    {
        public int Min { private get; set; }

        public int Max { private get; set; }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            double v;

            if (Double.TryParse((string) value, out v))
            {
                if ((v < Min) || (v > Max))
                {
                    return new ValidationResult(false, "Please enter a value in the range: " + Min + " - " + Max + ".");
                }

                return new ValidationResult(true, null);
            }

            else
            {
                return new ValidationResult(false, "Please enter only numeric values.");
            }
        }
    }
}
