using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    abstract class AbstractToolViewModel
    {
        public AbstractToolViewModel()
        {
            _standardizationViewModel.CancelCommand = CancelStandardizationCommand;
            _standardizationViewModel.ApplyCommand = ApplyStandardizationCommand;
            _standardizationViewModel.OkayCommand = OkayStandardizationCommand;

            _standardizationView.DataContext = _standardizationViewModel;
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

        protected ICommand _standardizationSelectionCommand;
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

        public ICommand StandardizationSelectionCommand
        {
            get
            {
                if (_standardizationSelectionCommand == null)
                {
                    _standardizationSelectionCommand = new RelayCommand(
                        param => this.DoStandardizationSelectionCommand(),
                        param => HasCriteriaSelected
                    );
                }
                return _standardizationSelectionCommand;
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
       protected abstract void DoStandardizationSelectionCommand();
       protected abstract void DoSendToInMemoryWorkspaceCommand();
       protected abstract void DoExportAsCSVCommand();
       protected abstract void DoDistributionCommand();
       protected abstract void DoClosingCommand();


       #region standardization

       protected StandardizationSelectionView _standardizationView = new StandardizationSelectionView();
       protected StandardizationSelectionViewModel _standardizationViewModel = new StandardizationSelectionViewModel();

       private ICommand _applyStandardizationCommand;
       private ICommand _okayStandardizationCommand;
       private ICommand _cancelStandardizationCommand;

       private ICommand CancelStandardizationCommand
       {
           get
           {
               if (_cancelStandardizationCommand == null)
               {
                   _cancelStandardizationCommand = new RelayCommand(
                       p => this.DoCancelStandardizationCommand(),
                       p => true);
               }
               return _cancelStandardizationCommand;
           }
       }

       private ICommand OkayStandardizationCommand
       {
           get
           {
               if (_okayStandardizationCommand == null)
               {
                   _okayStandardizationCommand = new RelayCommand(
                       p => this.DoOkayStandardizationCommand(),
                       p => true);
               }
               return _okayStandardizationCommand;
           }
       }

       private ICommand ApplyStandardizationCommand
       {
           get
           {
               if (_applyStandardizationCommand == null)
               {
                   _applyStandardizationCommand = new RelayCommand(
                       p => this.DoApplyStandardizationCommand(),
                       p => true);
               }
               return _applyStandardizationCommand;
           }
       }

       protected abstract void DoApplyStandardizationCommand();
       protected abstract void DoCancelStandardizationCommand();
       protected abstract void DoOkayStandardizationCommand();

       #endregion
    }
}
