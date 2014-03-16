using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MCDA.Model;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    internal sealed class ConfigViewModel
    {
        public event EventHandler RequestClose;

        private ICommand _applyCommand;
        private ICommand _okayCommand;
        private ICommand _cancelCommand;
        private RenderOption _seletectedRenderOption;

        public ConfigViewModel()
        {
            SelectedRenderOption = ConfigSingleton.Instance.SelectedRenderoption;
        }

        public RenderOption SelectedRenderOption
        {
            get { return _seletectedRenderOption; }
            set {_seletectedRenderOption = value;}
        }

        public ICommand CancelCommand
        {
            get
            {
                if (_cancelCommand == null)
                    _cancelCommand = new RelayCommand( p => this.DoCancelNormalizationCommand(), p => true);

                return _cancelCommand;
            }
        }

        public ICommand OkayCommand
        {
            get
            {
                if (_okayCommand == null)
                    _okayCommand = new RelayCommand( p => this.DoOkayCommand(), p => true);
                
                return _okayCommand;
            }
        }

        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                    _applyCommand = new RelayCommand(p => this.DoApplyCommand(), p => true);

                return _applyCommand;
            }
        }

        private void DoApplyCommand()
        {
            ConfigSingleton.Instance.SelectedRenderoption = SelectedRenderOption;
        }

        private void DoOkayCommand()
        {
            ConfigSingleton.Instance.SelectedRenderoption = SelectedRenderOption;
            RequestClose(null, null);
        }

        private void DoCancelNormalizationCommand()
        {
            RequestClose(null, null);
        }
    }
}
