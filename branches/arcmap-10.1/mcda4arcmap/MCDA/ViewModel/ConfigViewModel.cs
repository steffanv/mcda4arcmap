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

        public ConfigViewModel()
        {
            SelectedRenderOption = ConfigSingleton.Instance.SelectedRenderoption;
        }

        public RenderOption SelectedRenderOption { get; set; }

        public ICommand CancelCommand
        {
            get {
                return _cancelCommand ?? (_cancelCommand = new RelayCommand(p => this.DoCancelCommand(), p => true));
            }
        }

        public ICommand OkayCommand
        {
            get { return _okayCommand ?? (_okayCommand = new RelayCommand(p => this.DoOkayCommand(), p => true)); }
        }

        public ICommand ApplyCommand
        {
            get { return _applyCommand ?? (_applyCommand = new RelayCommand(p => this.DoApplyCommand(), p => true)); }
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

        private void DoCancelCommand()
        {
            RequestClose(null, null);
        }
    }
}
