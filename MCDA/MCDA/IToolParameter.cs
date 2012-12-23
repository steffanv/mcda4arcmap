using System;
using System.ComponentModel;

namespace MCDA.Model
{
    public interface IToolParameter : IDeepClonable<IToolParameter>, INotifyPropertyChanged
    {

        IToolParameter LastWeightChangedToolParameter { get; set; }

        string ColumnName { get; set; }

        double Weight {get; set;}

        double ScaledWeight { get; set; }

        bool IsLocked { get; set; }

        bool IsOID { get; set; }

        bool IsBenefitCriterion { get; set; }

    }
}
