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

        private ICommand _neighborhoodSelectionCommand;

        public LWLCToolViewModel()
        {
            _mcdaExtension = MCDAExtension.GetExtension();

            _lwlcTool = ToolFactory.NewLWLCTool();

            _lwlcResultDataTable = _lwlcTool.Data;

            _mcdaExtension.RegisterPropertyHandler(x => x.AvailableLayer, MCDAExtensionPropertyChanged);

            if (_mcdaExtension.SelectedLayer != null)
                _mcdaExtension.SelectedLayer.Fields.ForEach(x => x.RegisterPropertyHandler(f => f.IsSelected, FieldPropertyChanged));

            //we have to call our own update method to make sure we have a result column
            MCDAExtensionPropertyChanged(this, null);

        }

        private void WeightChanged(object sender, PropertyChangedEventArgs e)
        {
            base.Update();
        }

        private void BenefitCriterionChanged(object sender, PropertyChangedEventArgs e)
        {
            UpdateRealtime();
        }

        private void FieldPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_isLocked)
                return;

            _lwlcTool = ToolFactory.NewLWLCTool();

            _toolParameter = new BindingList<IToolParameter>(_lwlcTool.ToolParameterContainer.ToolParameter);

            _lwlcTool.Run();

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

            _lwlcTool.Run();

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

            _lwlcTool.Run();
            _lwlcResultDataTable = _lwlcTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _lwlcTool, _lwlcTool.Data);

            _isUpdateAllowed = false;

        }

        protected override void UpdateRealtime()
        {
            _lwlcTool.Run();
            _lwlcResultDataTable = _lwlcTool.Data;

            if (_isSendToInMemoryWorkspaceCommand)
                ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _lwlcTool, _lwlcTool.Data);
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
                            ProgressDialog.ShowProgressDialog("Creating Symbology", (Action<AbstractToolTemplate, DataTable>)_mcdaExtension.JoinToolResultByOID, _lwlcTool, _lwlcTool.Data);
                    }
                }

                //make sure we add the latest one
                _lwlcTool.ToolParameterContainer.ToolParameter = latestToolParameter;
                _lwlcTool.Run();
                _lwlcResultDataTable = _lwlcTool.Data;

                if (_isSendToInMemoryWorkspaceCommand)
                    _mcdaExtension.JoinToolResultByOID(_lwlcTool, _lwlcTool.Data);

                _isUpdateAllowed = false;

                _toolParameterStorageForAnimationLikeUpdate.Clear();
            }
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
                Export.ToCSV(_lwlcTool.Data, _lwlcTool.ToolParameterContainer.ToolParameter, saveFileDialog.FileName);
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
                ProgressDialog.ShowProgressDialog("Creating In Memory Representation", (Action<AbstractToolTemplate>)_mcdaExtension.EstablishLink, _lwlcTool);

            if (!_isLocked)
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

            var wpfWindow = new StandardizationSelectionView();

            StandardizationSelectionViewModel standardizationSelectionViewModel = wpfWindow.DataContext as StandardizationSelectionViewModel;

            standardizationSelectionViewModel.SelectedTransformationStrategy = _lwlcTool.TransformationStrategy;

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Closed += delegate(object sender, EventArgs e)
            {

                _lwlcTool.TransformationStrategy = standardizationSelectionViewModel.SelectedTransformationStrategy;

                _lwlcTool.Run();
                _lwlcResultDataTable = _lwlcTool.Data;

                _isUpdateAllowed = true;

                base.Update();

                PropertyChanged.Notify(() => LWLCResult);

            };

            wpfWindow.ShowDialog();
        }

        protected override void DoDistributionCommand()
        {
            _lwlcTool.ToolParameterContainer.DistributeEquallyToolParameterWeights();

            _lwlcTool.Run();

            _lwlcResultDataTable = _lwlcTool.Data;

            PropertyChanged.Notify(() => LWLCParameter);
            PropertyChanged.Notify(() => LWLCResult);

            UpdateRealtime();
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

            var wpfWindow = new NeighborhoodSelectionView();

            NeighborhoodSelectionViewModel neighborhoodSelectionViewModel = wpfWindow.DataContext as NeighborhoodSelectionViewModel;

            neighborhoodSelectionViewModel.NeighborhoodOption = _lwlcTool.NeighborhoodOptions;
            neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighbors = _lwlcTool.NumberOfKNearestNeighbors;
            neighborhoodSelectionViewModel.Threshold = _lwlcTool.Threshold;

            var helper = new WindowInteropHelper(wpfWindow);

            helper.Owner = parentHandle;

            wpfWindow.Closed += delegate(object sender, EventArgs e)
            {

                _lwlcTool.NeighborhoodOptions = neighborhoodSelectionViewModel.NeighborhoodOption;
                _lwlcTool.NumberOfKNearestNeighbors = neighborhoodSelectionViewModel.SelectedNumberOfKNearestNeighbors;
                _lwlcTool.Threshold = neighborhoodSelectionViewModel.Threshold;

                _isUpdateAllowed = true;

                base.Update();

                PropertyChanged.Notify(() => LWLCResult);

            };

            wpfWindow.ShowDialog();
        }
    }
}
