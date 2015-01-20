using System;
using System.ComponentModel;
using MCDA.Misc;

namespace MCDA.Model
{
    internal interface IToolParameter : IDeepClonable<IToolParameter>, INotifyPropertyChanged
    {
        IToolParameter LastWeightChangedToolParameter { get; set; }

        string ColumnName { get; set; }

        double Weight {get; set;}

        double ScaledWeight { get; set; }

        bool IsLocked { get; set; }

        bool IsOID { get; set; }

        bool IsBenefitCriterion { get; set; }

        /// <summary>
        /// The property indicates if the parameter is suitable for changes in the context of the container.
        /// </summary>
        bool IsActive { get; set; }

        ToolParameterContainer ToolParameterContainer { get; set; }

        /// <summary>
        /// The range of acceptable weights in the context of the container such that the sum of the weight of all parameter is equals 100.
        /// </summary>
        Range<double> AcceptableWeightRange { get; }
    }
}
