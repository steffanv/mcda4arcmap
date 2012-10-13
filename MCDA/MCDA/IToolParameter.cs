using System;

namespace MCDA.Model
{
    public interface IToolParameter
    {

        IToolParameter LastWeightChangedToolParameter { get; set; }
        bool IsPropertiesLocked { get; set; }

        string ColumnName { get; set; }

        double Weight {get; set;}

        double ScaledWeight { get; set; }

        void SetLockedWeight(double weight);

        bool IsLocked { get; set; }

        bool IsOID { get; set; }

        bool IsBenefitCriterion { get; set; }

    }
}
