using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Model;
using MCDA.Extensions;
using System.Data;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Windows.Input;
using System.Linq.Expressions;

namespace MCDA.ViewModel
{
    class OWAToolViewModel : AbstractToolViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MCDAExtension _mcdaExtension;
        private OWATool _owaTool;
        private DataTable _owaResultDataTable;
        private BindingList<IToolParameter> _toolParameter;
        private IList<List<IToolParameter>> _toolParameterStorageForAnimationLikeUpdate = new List<List<IToolParameter>>();

        private bool _isLocked = false;
        private bool _isSendToInMemoryWorkspaceCommand = false;
        private bool _isUpdateAllowed = false;

        private AlphaSelectionView _alphaSelectionView;
        private AlphaSelectionViewModel _alphaSelectionViewModel = new AlphaSelectionViewModel();
        private ICommand _alphaSelectionCommand;

        private ICommand _applyAlphaSelectionCommand;
        private ICommand _okayAlphaSelectionCommand;
        private ICommand _cancelAlphaSelectionCommand;

        public OWAToolViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _owaTool = ToolFactory.NewOWATool();

            _owaResultDataTable = _owaTool.Data;

            _mcdaExtension.RegisterPropertyHandler(x => x.AvailableLayer, MCDAExtensionPropertyChanged);

            //we have to call our own update method to make sure we have a result column
            MCDAExtensionPropertyChanged(this, null);

            // init stuff for the alpha selection
            // all commands are defined in this class and set here
            _alphaSelectionViewModel.CancelCommand = CancelAlphaSelectionCommand;
            _alphaSelectionViewModel.ApplyCommand = ApplyAlphaSelectionCommand;
            _alphaSelectionViewModel.OkayCommand = OkayAlphaSelectionCommand;
        }

        private void WeightChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Update();
        }

        private void BenefitCriterionChanged(object sender, PropertyChangedEventArgs e)
        {
            _isUpdateAllowed = true;

            base.Update();
        }

        private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
                return;

            _owaTool = ToolFactory.NewOWATool();

            _toolParameter = new BindingList<IToolParameter>(_owaTool.ToolParameterContainer.ToolParameter);

           

            if (_mcdaExtension.SelectedLayer.Fields.Count(f => f.IsSelected) >= 1){
                HasCriteriaSelected = true;

                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;
            }
            else
                HasCriteriaSelected = false;

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);
        }

        private void RegisterToolParameterEvents()
        {
            _toolParameter.ForEach(t => t.UnRegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
            _toolParameter.ForEach(t => t.UnRegisterPropertyHandler(w => w.Weight, WeightChanged));

            _toolParameter.ForEach(t => t.RegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
            _toolParameter.ForEach(t => t.RegisterPropertyHandler(w => w.Weight, WeightChanged));
        }

        private void MCDAExtensionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
                return;

            _owaTool = ToolFactory.NewOWATool();

            _toolParameter = new BindingList<IToolParameter>(_owaTool.ToolParameterContainer.ToolParameter);

            

            if (_mcdaExtension.SelectedLayer != null)
            {
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.UnRegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));

                if (_mcdaExtension.SelectedLayer.Fields.Count(f => f.IsSelected) >= 1){
                    HasCriteriaSelected = true;

                    _owaTool.Run();
                    _owaResultDataTable = _owaTool.Data;
                }
                else
                    HasCriteriaSelected = false;
            }

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);
        }

        //called from the code behind page if something changed
        public void UpdateAllowedEvent()
        {
            _isUpdateAllowed = true;
            base.Update();
        }

        protected override void UpdateDrag()
        {
            if (!_isUpdateAllowed)
                return;

            _owaTool.Run();
            _owaResultDataTable = _owaTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);

            _isUpdateAllowed = false;
        }

        protected override void UpdateRealtime()
        {
            _owaTool.Run();
            _owaResultDataTable = _owaTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);
        }

        protected override void UpdateAnimation()
        {
            if (!_isUpdateAllowed)
            {
                List<IToolParameter> tList = new List<IToolParameter>();

                for (int i = 0; i < _toolParameter.Count; i++)
                {
                    tList.Add(_toolParameter[i].DeepClone());
                }

                _toolParameterStorageForAnimationLikeUpdate.Add(tList);
            }

            else
            {
                BindingList<IToolParameter> latestToolParameter = _toolParameter;

                if (_toolParameterStorageForAnimationLikeUpdate.Count > 0)
                {
                    int steps = (int)Math.Sqrt(_toolParameterStorageForAnimationLikeUpdate.Count);
                    //take several steps...
                    for (int i = 0; i < _toolParameterStorageForAnimationLikeUpdate.Count; i = i + steps)
                    {
                        _owaTool.ToolParameterContainer.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                        _owaTool.Run();
                        _owaResultDataTable = _owaTool.Data;

                        if (_isSendToInMemoryWorkspaceCommand)
                            _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);
                    }
                }

                //make sure we add the latest one
                _owaTool.ToolParameterContainer.ToolParameter = latestToolParameter;
                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;

                if (_isSendToInMemoryWorkspaceCommand)
                    _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);

                _isUpdateAllowed = false;

                _toolParameterStorageForAnimationLikeUpdate.Clear();
            }
        }

        protected override void AfterUpdate()
        {
            PropertyChanged.Notify(() => OWAResult);
        }

        public BindingList<IToolParameter> OWAParameter
        {
            get { return _toolParameter; }
            set { _toolParameter = value; }
        }
        public DataView OWAResult
        {
            get { return _owaResultDataTable.DefaultView; }
        }

        public bool IsLocked
        {
            get { return _isLocked; }
            set { _isLocked = value; }
        }

        public bool IsSendToInMemoryWorkspaceCommand
        {
            get { return _isSendToInMemoryWorkspaceCommand; }
            set { _isSendToInMemoryWorkspaceCommand = value; }
        }

        protected override void DoExportAsCSVCommand()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = _owaTool.ToString();
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
                Export.ToCSV<IToolParameter>(_owaTool.Data, _owaTool.ToolParameterContainer.ToolParameter, saveFileDialog.FileName, Tuple.Create<string,object>(Util.GetPropertyName(() => _owaTool.Alpha), _owaTool.Alpha));
        }

        protected override void DoLockCommand()
        {
            _isLocked = !_isLocked;

            if (!_isLocked && _isSendToInMemoryWorkspaceCommand)
            {
                ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
                bool userResult = msgBox.DoModal("Unlocking", "Unlocking also removes the existing in memory connection.", "Yes", "No", ArcMap.Application.hWnd);

                //if the user hit no we have to set the lock state back to locked
                if (!userResult)
                {
                    _isLocked = !_isLocked;
                    return;
                }
                if (userResult)
                {
                    _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;
                    _mcdaExtension.RemoveLink(_owaTool);
                    this.MCDAExtensionPropertyChanged(this, null);
                }

                PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
            }

            if (_isLocked)
                ProgressDialog.ShowProgressDialog("Creating In Memory Representation", (Action<AbstractToolTemplate>)_mcdaExtension.EstablishLink, _owaTool);

            if (!_isLocked && !_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.RemoveLink(_owaTool);
                this.MCDAExtensionPropertyChanged(this, null);
            }

            PropertyChanged.Notify(() => IsLocked);
        }

        protected override void DoSendToInMemoryWorkspaceCommand()
        {
            _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;

            if (_isSendToInMemoryWorkspaceCommand && !_isLocked)
                DoLockCommand();

            if (_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.DisplayLink(_owaTool);
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);
            }

            if (!_isSendToInMemoryWorkspaceCommand)
            {
                //_mcdaExtension.RemoveLink(_owaTool);
                DoLockCommand();
            }

            PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
        }

        protected override void DoStandardizationSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            _standardizationView = new StandardizationSelectionView();

            _standardizationView.DataContext = _standardizationViewModel;

            _standardizationViewModel.SelectedTransformationStrategy = _owaTool.TransformationStrategy;

            var helper = new WindowInteropHelper(_standardizationView);

            helper.Owner = parentHandle;

            _standardizationView.ShowDialog();

            _standardizationView.Closed += delegate(object sender, EventArgs e)
           {
               DoCancelStandardizationCommand();
           };
        }

        protected override void DoApplyStandardizationCommand()
        {
            _owaTool.TransformationStrategy = _standardizationViewModel.SelectedTransformationStrategy;

            _isUpdateAllowed = true;
            base.Update();
        }

        protected override void DoCancelStandardizationCommand()
        {
            _standardizationViewModel.SelectedTransformationStrategy = _owaTool.TransformationStrategy;
            _standardizationView.Close();
        }

        protected override void DoOkayStandardizationCommand()
        {
            if (_owaTool.TransformationStrategy != _standardizationViewModel.SelectedTransformationStrategy)
                DoApplyStandardizationCommand();

            _standardizationView.Close();
        }

        public ICommand AlphaSelectionCommand
        {
            get
            {
                if (_alphaSelectionCommand == null)
                {
                    _alphaSelectionCommand = new RelayCommand(
                        p => this.DoAlphaSelectionCommand(),
                        p => HasCriteriaSelected
                        );
                }

                return _alphaSelectionCommand;
            }
        }

        public void DoAlphaSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            _alphaSelectionView = new AlphaSelectionView();

            _alphaSelectionViewModel.Alpha = _owaTool.Alpha;

            _alphaSelectionView.DataContext = _alphaSelectionViewModel;

            var helper = new WindowInteropHelper(_alphaSelectionView);

            helper.Owner = parentHandle;

            _alphaSelectionView.ShowDialog();

            _alphaSelectionView.Closing += AlphaSelectionViewClosing;

        }

        void AlphaSelectionViewClosing(object sender, CancelEventArgs e)
        {
            _alphaSelectionViewModel.Alpha = _owaTool.Alpha;
        }

        private ICommand ApplyAlphaSelectionCommand
        {
            get
            {
                if (_applyAlphaSelectionCommand == null)
                {
                    _applyAlphaSelectionCommand = new RelayCommand(
                        p => this.DoApplyAlphaSelectionCommand(),
                        p => true);
                }
                return _applyAlphaSelectionCommand;
            }
        }

        private void DoApplyAlphaSelectionCommand()
        {
            _owaTool.Alpha = _alphaSelectionViewModel.Alpha;

            _isUpdateAllowed = true;

            base.Update();
        }

        private ICommand OkayAlphaSelectionCommand
        {
            get
            {
                if (_okayAlphaSelectionCommand == null)
                {
                    _okayAlphaSelectionCommand = new RelayCommand(
                        p => this.DoOkayAlphaSelectionCommand(),
                        p => true);
                }
                return _okayAlphaSelectionCommand;
            }
        }

        private void DoOkayAlphaSelectionCommand()
        {
            if (_owaTool.Alpha != _alphaSelectionViewModel.Alpha)
                DoApplyAlphaSelectionCommand();

            _alphaSelectionView.Closing -= AlphaSelectionViewClosing;
            _alphaSelectionView.Close();
        }

        private ICommand CancelAlphaSelectionCommand
        {
            get
            {
                if (_cancelAlphaSelectionCommand == null)
                {
                    _cancelAlphaSelectionCommand = new RelayCommand(
                        p => this.DoCancelAlphaSelectionCommand(),
                        p => true);
                }
                return _cancelAlphaSelectionCommand;
            }
        }

        private void DoCancelAlphaSelectionCommand()
        {
            _alphaSelectionViewModel.Alpha = _owaTool.Alpha;

            _alphaSelectionView.Closing -= AlphaSelectionViewClosing;
            _alphaSelectionView.Close();
        }

        protected override void DoDistributionCommand()
        {
            _owaTool.ToolParameterContainer.DistributeEquallyToolParameterWeights();

            _isUpdateAllowed = true;
            base.Update();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);
        }

        protected override void DoClosingCommand()
        {
            if (_isLocked || _isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.RemoveLink(_owaTool); 
            }
        }
    }
}
