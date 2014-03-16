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
        public AbstractToolViewModel()
        {
            _NormalizationViewModel.CancelCommand = CancelNormalizationCommand;
            _NormalizationViewModel.ApplyCommand = ApplyNormalizationCommand;
            _NormalizationViewModel.OkayCommand = OkayNormalizationCommand;

            _NormalizationView.DataContext = _NormalizationViewModel;
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

        protected bool HasCriteriaSelected = false;

        public ICommand LockCommand
        {
            get
            {
                if (_lockCommand == null)
                {
                    _lockCommand = new RelayCommand(
                        p => this.DoLockCommand(),
                        p => HasCriteriaSelected
                        );
                }

                return _lockCommand;
            }
        }

        public ICommand NormalizationSelectionCommand
        {
            get
            {
                if (_NormalizationSelectionCommand == null)
                {
                    _NormalizationSelectionCommand = new RelayCommand(
                        param => this.DoNormalizationSelectionCommand(),
                        param => HasCriteriaSelected
                    );
                }
                return _NormalizationSelectionCommand;
            }
        }

        public ICommand SendToInMemoryWorkspaceCommand
        {
            get
            {
                if (_sendToInMemoryWorkspaceCommand == null)
                {

                    _sendToInMemoryWorkspaceCommand = new RelayCommand(
                        param => this.DoSendToInMemoryWorkspaceCommand(),
                        param => HasCriteriaSelected
                    );
                }
                return _sendToInMemoryWorkspaceCommand;
            }
        }

        public ICommand ExportAsCSVCommand
        {
            get
            {
               if (_exportAsCSVCommand == null)
                {
                    _exportAsCSVCommand = new RelayCommand(
                        p => this.DoExportAsCSVCommand(),
                        p => HasCriteriaSelected
                        );
                }

                return _exportAsCSVCommand;
            }
        }

        public ICommand DistributionCommand
        {
            get
            {
                if (_distributionCommand == null)
                {
                    _distributionCommand = new RelayCommand(
                        p => this.DoDistributionCommand(),
                        p => HasCriteriaSelected
                        );
                }

                return _distributionCommand;
            }
        }

        public ICommand ClosingCommand
        {
            get
            {

                if (_closingCommand == null)
                {
                    _closingCommand = new RelayCommand(
                        p => this.DoClosingCommand(),
                        p => true
                        );
                }

                return _closingCommand;
            }
        }

       protected abstract void DoLockCommand();
       protected abstract void DoNormalizationSelectionCommand();
       protected abstract void DoSendToInMemoryWorkspaceCommand();
       protected abstract void DoExportAsCSVCommand();
       protected abstract void DoDistributionCommand();
       protected abstract void DoClosingCommand();


       #region Normalization

       protected NormalizationSelectionView _NormalizationView = new NormalizationSelectionView();
       protected NormalizationSelectionViewModel _NormalizationViewModel = new NormalizationSelectionViewModel();

       private ICommand _applyNormalizationCommand;
       private ICommand _okayNormalizationCommand;
       private ICommand _cancelNormalizationCommand;

       private ICommand CancelNormalizationCommand
       {
           get
           {
               if (_cancelNormalizationCommand == null)
               {
                   _cancelNormalizationCommand = new RelayCommand(
                       p => this.DoCancelNormalizationCommand(),
                       p => true);
               }
               return _cancelNormalizationCommand;
           }
       }

       private ICommand OkayNormalizationCommand
       {
           get
           {
               if (_okayNormalizationCommand == null)
               {
                   _okayNormalizationCommand = new RelayCommand(
                       p => this.DoOkayNormalizationCommand(),
                       p => true);
               }
               return _okayNormalizationCommand;
           }
       }

       private ICommand ApplyNormalizationCommand
       {
           get
           {
               if (_applyNormalizationCommand == null)
               {
                   _applyNormalizationCommand = new RelayCommand(
                       p => this.DoApplyNormalizationCommand(),
                       p => true);
               }
               return _applyNormalizationCommand;
           }
       }

       protected abstract void DoApplyNormalizationCommand();
       protected abstract void DoCancelNormalizationCommand();
       protected abstract void DoOkayNormalizationCommand();

       #endregion
    }
}
