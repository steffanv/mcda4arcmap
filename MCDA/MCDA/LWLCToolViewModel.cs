using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using MCDA.Model;
using MCDA.Extensions;
using System.Data;
using System.Windows.Input;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Framework;
using System.Windows.Interop;
using Microsoft.Win32;

namespace MCDA.ViewModel
{
    class LWLCToolViewModel : AbstractToolViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private MCDAExtension _mcdaExtension;
        private LWLCTool _lwlcTool;
        private DataTable _lwlcResultDataTable;
        private BindingList<IToolParameter> _toolParameter;
        private IList<List<IToolParameter>> _toolParameterStorageForAnimationLikeUpdate = new List<List<IToolParameter>>();

        private bool _isLocked = false;
        private bool _isSendToInMemoryWorkspaceCommand = false;
        private bool _isUpdateAllowed = false;

        private NeighborhoodSelectionView _neighborhoodSelectionView;
        private NeighborhoodSelectionViewModel _neighborhoodSelectionViewModel = new NeighborhoodSelectionViewModel();
        private ICommand _neighborhoodSelectionCommand;

        private ICommand _applyNeighborhoodSelectionCommand;
        private ICommand _okayNeighborhoodSelectionCommand;
        private ICommand _cancelNeighborhoodSelectionCommand;

        public LWLCToolViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _lwlcTool = ToolFactory.NewLWLCTool();

            _lwlcResultDataTable = _lwlcTool.Data;

            _mcdaExtension.RegisterPropertyHandler(x => x.AvailableLayer, MCDAExtensionPropertyChanged);

            //we have to call our own update method to make sure we have a result column
            MCDAExtensionPropertyChanged(this, null);

            // init stuff for the neigborhood selection
            // all commands are defined in this class and set here
            _neighborhoodSelectionViewModel.OkayCommand = OkayNeighborhoodSelectionCommand;
            _neighborhoodSelectionViewModel.CancelCommand = CancelNeighborhoodSelectionCommand;
            _neighborhoodSelectionViewModel.ApplyCommand = ApplyNeighborhoodSelectionCommand;

            _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighbors = 3;
            _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighborsForAutomatic = 3;

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

            _lwlcTool = ToolFactory.NewLWLCTool();

            _toolParameter = new BindingList<IToolParameter>(_lwlcTool.ToolParameterContainer.ToolParameter);

            ProgressDialog.ShowProgressDialog("Running LWLC Tool", (Action)_lwlcTool.Run);

            _lwlcResultDataTable = _lwlcTool.Data;

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => LWLCParameter);
            PropertyChanged.Notify(() => LWLCResult);
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

            _lwlcTool = ToolFactory.NewLWLCTool();

            _toolParameter = new BindingList<IToolParameter>(_lwlcTool.ToolParameterContainer.ToolParameter);

            ProgressDialog.ShowProgressDialog("Running LWLC Tool", (Action)_lwlcTool.Run);

            _lwlcResultDataTable = _lwlcTool.Data;

            if (_mcdaExtension.SelectedLayer != null)
            {
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.UnRegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
            }

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => LWLCParameter);
            PropertyChanged.Notify(() => LWLCResult);
        }

        // called from the code behind page if something changed
        public void UpdateAllowedEvent(bool isUpdateAllowed)
        {
            _isUpdateAllowed = isUpdateAllowed;
        }

        protected override void UpdateDrag()
        {
            if (!_isUpdateAllowed)
                return;

            ProgressDialog.ShowProgressDialog("Running LWLC Tool", (Action)_lwlcTool.Run);
            _lwlcResultDataTable = _lwlcTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _lwlcTool, _lwlcTool.Data);

            _isUpdateAllowed = false;
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
                        _lwlcTool.ToolParameterContainer.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                        _lwlcTool.Run();
                        _lwlcResultDataTable = _lwlcTool.Data;

                        if (_isSendToInMemoryWorkspaceCommand)
                            _mcdaExtension.JoinToolResultByOID(_lwlcTool, _lwlcTool.Data);
                    }
                }

                //make sure we add the latest one
                _lwlcTool.ToolParameterContainer.ToolParameter = latestToolParameter;
                _lwlcTool.Run();
                _lwlcResultDataTable = _lwlcTool.Data;

                if (_isSendToInMemoryWorkspaceCommand)
                    _mcdaExtension.JoinToolResultByOID(_lwlcTool, _lwlcTool.Data);

                //_isUpdateAllowed = false;

                _toolParameterStorageForAnimationLikeUpdate.Clear();
            }
        }

        protected override void UpdateRealtime()
        {
            _lwlcTool.Run();
            _lwlcResultDataTable = _lwlcTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                _mcdaExtension.JoinToolResultByOID(_lwlcTool, _lwlcTool.Data);
        }

        protected override void AfterUpdate()
        {
            PropertyChanged.Notify(() => LWLCResult);
        }

        public BindingList<IToolParameter> LWLCParameter
        {
            get { return _toolParameter; }
            set { _toolParameter = value; }
        }
        public DataView LWLCResult
        {
            get { return _lwlcResultDataTable.DefaultView; }
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
            saveFileDialog.FileName = _lwlcTool.ToString();
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

            Nullable<bool> result = saveFileDialog.ShowDialog();

            if (result == true)
            {

                IList<Tuple<string, object>> param = new List<Tuple<string, object>>();

                param.Add(Tuple.Create<string,object>(Util.GetPropertyName(() => _lwlcTool.NeighborhoodOptions), _lwlcTool.NeighborhoodOptions));
                param.Add(Tuple.Create<string, object>(Util.GetPropertyName(() => _lwlcTool.NumberOfKNearestNeighbors), _lwlcTool.NumberOfKNearestNeighbors));
                param.Add(Tuple.Create<string, object>(Util.GetPropertyName(() => _lwlcTool.NumberOfKNearestNeighborsForAutomatic), _lwlcTool.NumberOfKNearestNeighborsForAutomatic));
                param.Add(Tuple.Create<string, object>(Util.GetPropertyName(() => _lwlcTool.Threshold), _lwlcTool.Threshold));

                Export.ToCSV<IToolParameter>(_lwlcTool.Data, _lwlcTool.ToolParameterContainer.ToolParameter, saveFileDialog.FileName, param.ToArray());
            }
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
                    _mcdaExtension.RemoveLink(_lwlcTool);
                    this.MCDAExtensionPropertyChanged(this, null);

                    PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
                }
            }

            if (_isLocked)
            {
                ProgressDialog.ShowProgressDialog("Creating In Memory Representation", (Action<AbstractToolTemplate>)_mcdaExtension.EstablishLink, _lwlcTool);
                // the lwlc depends on the fc data, thus we have to make sure that we work on the fc of the in memory workspace
                _lwlcTool.FeatureClass = _mcdaExtension.LinkDictionary[_lwlcTool].FeatureClass;
            }

            if (!_isLocked && !_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.RemoveLink(_lwlcTool);
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
                _mcdaExtension.DisplayLink(_lwlcTool);
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _lwlcTool, _lwlcTool.Data);
            }

            if (!_isSendToInMemoryWorkspaceCommand)
                //_mcdaExtension.RemoveLink(_wlcTool);
                DoLockCommand();

            PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
        }

        protected override void DoStandardizationSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            _standardizationView = new StandardizationSelectionView();

            _standardizationView.DataContext = _standardizationViewModel;

            _standardizationViewModel.SelectedTransformationStrategy = _lwlcTool.TransformationStrategy;

            var helper = new WindowInteropHelper(_standardizationView);

            helper.Owner = parentHandle;

            _standardizationView.ShowDialog();
        }

        protected override void DoApplyStandardizationCommand()
        {
            _lwlcTool.TransformationStrategy = _standardizationViewModel.SelectedTransformationStrategy;

            _isUpdateAllowed = true;
            base.Update();
        }

        protected override void DoCancelStandardizationCommand()
        {
            _standardizationViewModel.SelectedTransformationStrategy = _lwlcTool.TransformationStrategy;
            _standardizationView.Close();
        }

        protected override void DoOkayStandardizationCommand()
        {
            if (_lwlcTool.TransformationStrategy != _standardizationViewModel.SelectedTransformationStrategy)
                DoApplyStandardizationCommand();

            _standardizationView.Close();
        }

        protected override void DoDistributionCommand()
        {
            // in order to prevent the tool to run for every single step
            _isUpdateAllowed = false;

            _lwlcTool.ToolParameterContainer.DistributeEquallyToolParameterWeights();

            _isUpdateAllowed = true;
            base.Update();

            PropertyChanged.Notify(() => LWLCParameter);
            PropertyChanged.Notify(() => LWLCResult);      
        }

        protected override void DoClosingCommand()
        {
            if (_isLocked || _isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.RemoveLink(_lwlcTool);

            }
        }

        public ICommand NeighborhoodSelectionCommand
        {
            get
            {
                if (_neighborhoodSelectionCommand == null)
                {
                    _neighborhoodSelectionCommand = new RelayCommand(
                        p => this.DoNeighborhoodSelectionCommand(),
                        p => true
                        );
                }

                return _neighborhoodSelectionCommand;
            }
        }

        public void DoNeighborhoodSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            _neighborhoodSelectionView = new NeighborhoodSelectionView();

            _neighborhoodSelectionViewModel.ThresholdMax = _lwlcTool.ThresholdMax;
            _neighborhoodSelectionViewModel.ThresholdMin = _lwlcTool.ThresholdMin;

            _neighborhoodSelectionView.DataContext = _neighborhoodSelectionViewModel;

            var helper = new WindowInteropHelper(_neighborhoodSelectionView);

            helper.Owner = parentHandle;

            _neighborhoodSelectionView.ShowDialog();
        }

        private ICommand OkayNeighborhoodSelectionCommand
        {
            get
            {
                if (_okayNeighborhoodSelectionCommand == null)
                {
                    _okayNeighborhoodSelectionCommand= new RelayCommand(
                        p => this.DoOkayNeighborhoodSelectionCommand(),
                        p => true);
                }
                return _okayNeighborhoodSelectionCommand;
            }
        }

        private void DoOkayNeighborhoodSelectionCommand()
        {
            bool changed = false;

            if (_neighborhoodSelectionViewModel.NeighborhoodOption != _lwlcTool.NeighborhoodOptions)
                changed = true;
                  
            else {

                if(_neighborhoodSelectionViewModel.NeighborhoodOption == NeighborhoodOptions.Automatic && _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighborsForAutomatic != _lwlcTool.NumberOfKNearestNeighborsForAutomatic)
                     changed = true;
                if(_neighborhoodSelectionViewModel.NeighborhoodOption == NeighborhoodOptions.Threshold && _neighborhoodSelectionViewModel.Threshold != _lwlcTool.Threshold)
                    changed = true;
                if(_neighborhoodSelectionViewModel.NeighborhoodOption == NeighborhoodOptions.KNearestNeighbors && _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighbors != _lwlcTool.NumberOfKNearestNeighbors)
                    changed = true;
            }

            if(changed)
                DoApplyNeighborhoodSelectionCommand();

            _neighborhoodSelectionView.Close();
        }

        private ICommand CancelNeighborhoodSelectionCommand
        {
            get
            {
                if (_cancelNeighborhoodSelectionCommand == null)
                {
                    _cancelNeighborhoodSelectionCommand = new RelayCommand(
                        p => this.DoCancelNeighborhoodSelectionCommand(),
                        p => true);
                }
                return _cancelNeighborhoodSelectionCommand;
            }
        }

        private void DoCancelNeighborhoodSelectionCommand()
        {
            _neighborhoodSelectionViewModel.NeighborhoodOption = _lwlcTool.NeighborhoodOptions;
            _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighbors = _lwlcTool.NumberOfKNearestNeighbors;
            _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighborsForAutomatic = _lwlcTool.NumberOfKNearestNeighborsForAutomatic;
            _neighborhoodSelectionViewModel.Threshold = _lwlcTool.Threshold;

            _neighborhoodSelectionView.Close();
        }

        private ICommand ApplyNeighborhoodSelectionCommand
        {
            get
            {
                if (_applyNeighborhoodSelectionCommand == null)
                {
                    _applyNeighborhoodSelectionCommand = new RelayCommand(
                        p => this.DoApplyNeighborhoodSelectionCommand(),
                        p => true);
                }
                return _applyNeighborhoodSelectionCommand;
            }
        }

        private void DoApplyNeighborhoodSelectionCommand()
        {
            _lwlcTool.NeighborhoodOptions = _neighborhoodSelectionViewModel.NeighborhoodOption;
            _lwlcTool.NumberOfKNearestNeighbors = _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighbors;
            _lwlcTool.Threshold = _neighborhoodSelectionViewModel.Threshold;
            _lwlcTool.NumberOfKNearestNeighborsForAutomatic = _neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighborsForAutomatic;

            _isUpdateAllowed = true;
            base.Update();

            PropertyChanged.Notify(() => LWLCResult);
        }
    }
}
