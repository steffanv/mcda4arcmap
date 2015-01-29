using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using MCDA.Misc;
using MCDA.Model;
using MCDA.Extensions;
using System.Data;
using Microsoft.Win32;
using System.Windows.Interop;
using System.Windows.Input;

namespace MCDA.ViewModel
{
    internal sealed class OWAToolViewModel : AbstractToolViewModel, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly MCDAExtension _mcdaExtension;
        private OWATool _owaTool;
        private DataTable _owaResultDataTable;
        private BindingList<IToolParameter> _toolParameter;
        private readonly IList<List<IToolParameter>> _toolParameterStorageForAnimationLikeUpdate = new List<List<IToolParameter>>();
        private Feature _selectedFeature;

        private bool _isLocked = false;
        private bool _isSendToInMemoryWorkspaceCommand = false;
        private bool _isUpdateAllowed = false;

        private AlphaSelectionView _alphaSelectionView;
        private readonly AlphaSelectionViewModel _alphaSelectionViewModel = new AlphaSelectionViewModel();
        private ICommand _alphaSelectionCommand;

        private ICommand _applyAlphaSelectionCommand;
        private ICommand _okayAlphaSelectionCommand;
        private ICommand _cancelAlphaSelectionCommand;

        private readonly PropertyChangedEventHandler _selectedFeaturePropertyChangedEventHandler;

        private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion = new List<PropertyChangedEventHandler>();
        private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForToolParameterWeight = new List<PropertyChangedEventHandler>();
        private readonly IList<PropertyChangedEventHandler> _listOfpropertyChangedEventHandlersForFieldIsSelected = new List<PropertyChangedEventHandler>();

        public OWAToolViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _owaTool = ToolFactory.NewOWATool();

            _owaResultDataTable = _owaTool.Data;

           _selectedFeaturePropertyChangedEventHandler = _mcdaExtension.RegisterPropertyHandler(x => x.SelectedFeature, SelectedFeaturePropertyChanged);

            //we have to call our own update method to make sure we have a result column
            SelectedFeaturePropertyChanged(this, null);

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

        protected override bool HasCriteriaSelected()
        {
            return _selectedFeature != null && _selectedFeature.Fields.Count(f => f.IsSelected) >= 1;
        }

        private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
            {
                return;
            }

            _owaTool = ToolFactory.NewOWATool();

            _toolParameter = new BindingList<IToolParameter>(_owaTool.ToolParameterContainer.ToolParameter);

            if (_selectedFeature.Fields.Count(f => f.IsSelected) >= 1){      

                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;
            }

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);

            CommandManager.InvalidateRequerySuggested();
        }

        private void RegisterToolParameterEvents()
        {
            foreach (var currentToolParameter in _toolParameter)
            {
                currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion);
                currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterWeight);
            }

            foreach(var currentToolParameter in _toolParameter){

                _listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion.Add(currentToolParameter.RegisterPropertyHandler(b => b.IsBenefitCriterion, BenefitCriterionChanged));
                _listOfpropertyChangedEventHandlersForToolParameterWeight.Add(currentToolParameter.RegisterPropertyHandler(w => w.Weight, WeightChanged));
            }

        }

        private void SelectedFeaturePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
            {
                return;
            }

            _owaTool = ToolFactory.NewOWATool();

            _toolParameter = new BindingList<IToolParameter>(_owaTool.ToolParameterContainer.ToolParameter);

            if (_selectedFeature != null)
            {
                foreach (var currentField in _selectedFeature.Fields)
                {
                    currentField.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFieldIsSelected);
                }
            }

            _selectedFeature = _mcdaExtension.SelectedFeature;

            if (_selectedFeature != null)
            {
                foreach (var currentField in _selectedFeature.Fields)
                {
                    _listOfpropertyChangedEventHandlersForFieldIsSelected.Add(
                        currentField.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));
                }

                if (_selectedFeature.Fields.Count(f => f.IsSelected) >= 1)
                {
                    _owaTool.Run();
                    _owaResultDataTable = _owaTool.Data;
                }
            }

            RegisterToolParameterEvents();

            PropertyChanged.Notify(() => OWAParameter);
            PropertyChanged.Notify(() => OWAResult);

            CommandManager.InvalidateRequerySuggested();
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
            {
                return;
            }

            _owaTool.Run();
            _owaResultDataTable = _owaTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);
            }

            _isUpdateAllowed = false;
        }

        protected override void UpdateRealtime()
        {
            _owaTool.Run();
            _owaResultDataTable = _owaTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);
            }
        }

        protected override void UpdateAnimation()
        {
            if (!_isUpdateAllowed)
            {
                var tList = _toolParameter.Select(t => t.DeepClone()).ToList();

                _toolParameterStorageForAnimationLikeUpdate.Add(tList);
            }

            else
            {
                var latestToolParameter = _toolParameter;

                if (_toolParameterStorageForAnimationLikeUpdate.Count > 0)
                {
                    var steps = (int)Math.Sqrt(_toolParameterStorageForAnimationLikeUpdate.Count);
                    //take several steps...
                    for (var i = 0; i < _toolParameterStorageForAnimationLikeUpdate.Count; i = i + steps)
                    {
                        _owaTool.ToolParameterContainer.ToolParameter = _toolParameterStorageForAnimationLikeUpdate[i];
                        _owaTool.Run();
                        _owaResultDataTable = _owaTool.Data;

                        if (_isSendToInMemoryWorkspaceCommand)
                        {
                            _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);
                        }
                    }
                }

                //make sure we add the latest one
                _owaTool.ToolParameterContainer.ToolParameter = latestToolParameter;
                _owaTool.Run();
                _owaResultDataTable = _owaTool.Data;

                if (_isSendToInMemoryWorkspaceCommand)
                {
                    _mcdaExtension.JoinToolResultByOID(_owaTool, _owaTool.Data);
                }

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
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = _owaTool.ToString();
            saveFileDialog.DefaultExt = ".csv";
            saveFileDialog.Filter = "Comma Separated Values (.csv)|*.csv";

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {

                Export.ToCSV<IToolParameter>(_owaTool.Data, _owaTool.ToolParameterContainer.ToolParameter,
                    saveFileDialog.FileName,
                    Tuple.Create<string, object>(Util.GetPropertyName(() => _owaTool.Alpha), _owaTool.Alpha));
            }
        }

        protected override void DoLockCommand()
        {
            _isLocked = !_isLocked;

            if (!_isLocked && _isSendToInMemoryWorkspaceCommand)
            {
                ESRI.ArcGIS.Framework.IMessageDialog msgBox = new ESRI.ArcGIS.Framework.MessageDialogClass();
                var userResult = msgBox.DoModal("Unlocking", "Unlocking also disconnects from the managed layer.", "Unlock", "Abort", ArcMap.Application.hWnd);

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
                    this.SelectedFeaturePropertyChanged(this, null);
                }

                PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
            }

            if (_isLocked)
            {
                ProgressDialog.ShowProgressDialog("Creating In Memory Representation",
                    (Action<AbstractToolTemplate>) _mcdaExtension.EstablishLink, _owaTool);
            }

            if (!_isLocked && !_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.RemoveLink(_owaTool);
                this.SelectedFeaturePropertyChanged(this, null);
            }

            PropertyChanged.Notify(() => IsLocked);
        }

        protected override void DoSendToInMemoryWorkspaceCommand()
        {
            _isSendToInMemoryWorkspaceCommand = !_isSendToInMemoryWorkspaceCommand;

            if (_isSendToInMemoryWorkspaceCommand && !_isLocked)
            {
                DoLockCommand();
            }

            if (_isSendToInMemoryWorkspaceCommand)
            {
                _mcdaExtension.DisplayLink(_owaTool);
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _owaTool, _owaTool.Data);
            }

            if (!_isSendToInMemoryWorkspaceCommand)
            {
                DoLockCommand();
            }

            PropertyChanged.Notify(() => IsSendToInMemoryWorkspaceCommand);
        }

        protected override void DoNormalizationSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            NormalizationView = new NormalizationSelectionView { DataContext = NormalizationViewModel };

            NormalizationViewModel.SelectedTransformationStrategy = _owaTool.TransformationStrategy;

            var helper = new WindowInteropHelper(NormalizationView) { Owner = parentHandle };

            NormalizationView.ShowDialog();

            NormalizationView.Closed += (sender, e) => DoCancelNormalizationCommand();
        }

        protected override void DoApplyNormalizationCommand()
        {
            _owaTool.TransformationStrategy = NormalizationViewModel.SelectedTransformationStrategy;

            _isUpdateAllowed = true;
            base.Update();
        }

        protected override void DoCancelNormalizationCommand()
        {
            NormalizationViewModel.SelectedTransformationStrategy = _owaTool.TransformationStrategy;
            NormalizationView.Close();
        }

        protected override void DoOkayNormalizationCommand()
        {
            if (_owaTool.TransformationStrategy != NormalizationViewModel.SelectedTransformationStrategy)
                DoApplyNormalizationCommand();

            NormalizationView.Close();
        }

        public ICommand AlphaSelectionCommand
        {
            get
            {
                return _alphaSelectionCommand ?? (_alphaSelectionCommand = new RelayCommand(
                    p => this.DoAlphaSelectionCommand(),  p => HasCriteriaSelected() ));
            }
        }

        public void DoAlphaSelectionCommand()
        {
            var parentHandle = new IntPtr(ArcMap.Application.hWnd);

            _alphaSelectionView = new AlphaSelectionView();

            _alphaSelectionViewModel.Alpha = _owaTool.Alpha;

            _alphaSelectionView.DataContext = _alphaSelectionViewModel;

            var helper = new WindowInteropHelper(_alphaSelectionView) { Owner = parentHandle };

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
                return _applyAlphaSelectionCommand ?? (_applyAlphaSelectionCommand = new RelayCommand(
                    p => this.DoApplyAlphaSelectionCommand(), p => true));
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
                return _cancelAlphaSelectionCommand ?? (_cancelAlphaSelectionCommand = new RelayCommand(
                    p => this.DoCancelAlphaSelectionCommand(), p => true));
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

            _mcdaExtension.UnRegisterPropertyHandler(_selectedFeaturePropertyChangedEventHandler);

            if (_selectedFeature != null)
            {
                foreach (var currentField in _selectedFeature.Fields)
                {
                    currentField.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForFieldIsSelected);
                }
            }

            foreach (var currentToolParameter in _toolParameter)
            {
                currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterIsBenefitCriterion);
                currentToolParameter.UnRegisterPropertyHandler(_listOfpropertyChangedEventHandlersForToolParameterWeight);
            }
        }
    }
}
