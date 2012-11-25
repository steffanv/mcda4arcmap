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

        public ICommand LockCommand
        {
            get
            {

                if (_lockCommand == null)
                {
                    _lockCommand = new RelayCommand(
                        p => this.DoLockCommand(),
                        p => true
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
                        param => true
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
                        param => true
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
                        p => true
                        );
                }

                return _exportAsCSVCommand;
            }
        }

       protected abstract void DoLockCommand();
       protected abstract void DoStandardizationSelectionCommand();
       protected abstract void DoSendToInMemoryWorkspaceCommand();
       protected abstract void DoExportAsCSVCommand();

    }
}
