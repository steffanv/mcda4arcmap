using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    internal abstract class AbstractToolViewModel
    {
        protected AbstractToolViewModel()
        {
            NormalizationViewModel.CancelCommand = CancelNormalizationCommand;
            NormalizationViewModel.ApplyCommand = ApplyNormalizationCommand;
            NormalizationViewModel.OkayCommand = OkayNormalizationCommand;

            NormalizationView.DataContext = NormalizationViewModel;
        }

        protected virtual void BeforeUpdate(){}
        protected abstract void UpdateDrag();
        protected abstract void UpdateRealtime();
        protected abstract void UpdateAnimation();
        protected virtual void AfterUpdate(){}

        public void Update()
        {
            BeforeUpdate();

            switch (ConfigSingleton.Instance.SelectedRenderoption)
            {

                case RenderOption.AfterSliderDrag:
                    UpdateDrag();
                    break;
                case RenderOption.AnimationLike:
                    UpdateAnimation();
                    break;
                case RenderOption.Realtime:
                    UpdateRealtime();
                    break;
                default:
                    UpdateDrag();
                    break;

            }

            AfterUpdate();
   
        }

        protected ICommand _NormalizationSelectionCommand;
        protected ICommand _sendToInMemoryWorkspaceCommand;
        protected ICommand _lockCommand;
        protected ICommand _exportAsCSVCommand;
        protected ICommand _distributionCommand;
        protected ICommand _closingCommand;

        protected abstract bool HasCriteriaSelected();

        public ICommand LockCommand
        {
            get
            {
                return _lockCommand ?? (_lockCommand = new RelayCommand( p => this.DoLockCommand(), p => HasCriteriaSelected()));
            }
        }

        public ICommand NormalizationSelectionCommand
        {
            get
            {
                return _NormalizationSelectionCommand ?? (_NormalizationSelectionCommand = new RelayCommand( param => this.DoNormalizationSelectionCommand(), param => HasCriteriaSelected()));
            }
        }

        public ICommand SendToInMemoryWorkspaceCommand
        {
            get
            {
                return _sendToInMemoryWorkspaceCommand ?? (_sendToInMemoryWorkspaceCommand = new RelayCommand( param => this.DoSendToInMemoryWorkspaceCommand(), param => HasCriteriaSelected()));
            }
        }

        public ICommand ExportAsCSVCommand
        {
            get
            {
                return _exportAsCSVCommand ?? (_exportAsCSVCommand = new RelayCommand(p => this.DoExportAsCSVCommand(), p => HasCriteriaSelected() ));
            }
        }

        public ICommand DistributionCommand
        {
            get
            {
                return _distributionCommand ?? (_distributionCommand = new RelayCommand( p => this.DoDistributionCommand(), p => HasCriteriaSelected() ));
            }
        }

        public ICommand ClosingCommand
        {
            get
            {
                return _closingCommand ?? (_closingCommand = new RelayCommand( p => this.DoClosingCommand(),  p => true ));
            }
        }

       protected abstract void DoLockCommand();
       protected abstract void DoNormalizationSelectionCommand();
       protected abstract void DoSendToInMemoryWorkspaceCommand();
       protected abstract void DoExportAsCSVCommand();
       protected abstract void DoDistributionCommand();
       protected abstract void DoClosingCommand();


       #region Normalization

       protected NormalizationSelectionView NormalizationView = new NormalizationSelectionView();
       protected NormalizationSelectionViewModel NormalizationViewModel = new NormalizationSelectionViewModel();

       private ICommand _applyNormalizationCommand;
       private ICommand _okayNormalizationCommand;
       private ICommand _cancelNormalizationCommand;

       private ICommand CancelNormalizationCommand
       {
           get
           {
               return _cancelNormalizationCommand ?? (_cancelNormalizationCommand = new RelayCommand(p => this.DoCancelNormalizationCommand(), p => true));
           }
       }

       private ICommand OkayNormalizationCommand
       {
           get
           {
               return _okayNormalizationCommand ?? (_okayNormalizationCommand = new RelayCommand(p => this.DoOkayNormalizationCommand(), p => true));
           }
       }

       private ICommand ApplyNormalizationCommand
       {
           get
           {
               return _applyNormalizationCommand ?? (_applyNormalizationCommand = new RelayCommand( p => this.DoApplyNormalizationCommand(), p => true));
           }
       }

       protected abstract void DoApplyNormalizationCommand();
       protected abstract void DoCancelNormalizationCommand();
       protected abstract void DoOkayNormalizationCommand();

       #endregion
    }
}
